using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.OData.Formatter;
using Microsoft.CSharp;
using Microsoft.OData.Core;
using Microsoft.OData.Core.Aggregation;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using System.Web.Http;

namespace System.Web.OData.OData.Query
{
    public static class AggregationDynamicTypeCache
    {

        static AggregationDynamicTypeCache()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                if (assemblies.ContainsKey(e.Name))
                {
                    return assemblies[e.Name];
                }

                return null;
            };
        }

        internal static Dictionary<string, Type> ExistingTypes = new Dictionary<string, Type>();
        private static ConcurrentDictionary<string, object> SyncLocks = new ConcurrentDictionary<string, object>();

        private static Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

        public static void AddAssembly(Assembly newAssembly)
        {
            if (!assemblies.ContainsKey(newAssembly.FullName))
            {
                assemblies.Add(newAssembly.FullName, newAssembly);
            }
        }

        public static object GetLock(string key)
        {
            return SyncLocks.GetOrAdd(key, _ => new object());
        }
    }
    
    /// <summary>
    /// Helper class for creating new types dynamically 
    /// </summary>
    public class AggregationTypesGenerator
    {
        /// <summary>
        /// Creates a new type dynamically and register it in the EDM model
        /// </summary>
        /// <param name="properties">List of properties for the new type</param>
        /// <param name="context">The OData query context</param>
        /// <param name="asEdmEntity">Should the new type be defined as entity or complex type in the EDM model</param>
        /// <returns></returns>
        public static Type CreateType(List<Tuple<Type, string>> properties, ODataQueryContext context, bool asEdmEntity)
        {
            Contract.Assert(properties != null);
            Contract.Assert(context != null);
            Contract.Assert(properties.Any());

            string typeName = GenerateClassName(properties, asEdmEntity);
            var tempPath = Path.GetTempPath();
            string outputfile = Path.Combine(tempPath, typeName + ".dll");

            var lockHandle = AggregationDynamicTypeCache.GetLock(typeName);

            lock (lockHandle)
            {
                Type result;
                if (AggregationDynamicTypeCache.ExistingTypes.TryGetValue(typeName, out result))
                {
                    return result;
                }

                List<string> referencedAssemblies = new List<string>();
                foreach (var prop in properties)
                {
                    if (prop.Item1.Assembly.GetName().Name != "mscorlib")
                    {
                        if (!string.IsNullOrEmpty(prop.Item1.Assembly.Location))
                        {
                            referencedAssemblies.Add(prop.Item1.Assembly.Location);
                        }
                        else
                        {
                            referencedAssemblies.Add(Path.Combine(tempPath, prop.Item1.Name + ".dll"));
                        }
                    }
                }

                var code = CreateCode(typeName, properties);
                CSharpCodeProvider provider = new CSharpCodeProvider();
                CompilerParameters parameters = new CompilerParameters();
                parameters.GenerateInMemory = true;
                parameters.GenerateExecutable = false;
                parameters.OutputAssembly = outputfile;
                parameters.ReferencedAssemblies.AddRange(referencedAssemblies.Distinct().ToArray());
                parameters.TempFiles = new TempFileCollection(tempPath, false);
                CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

                if (results.Errors.HasErrors)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (CompilerError error in results.Errors)
                    {
                        sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                    }
                    throw Error.InvalidOperation(sb.ToString());
                }

                Assembly assembly = results.CompiledAssembly;
                AggregationDynamicTypeCache.AddAssembly(assembly);

                var newType = assembly.GetType("ODataAggregation.DynamicTypes." + typeName);

                if (context.Model.FindDeclaredType(newType.FullName) == null)
                {
                    var model = (EdmModel)context.Model;
                    IEdmSchemaElement entityType = CreateEdmSechmaElement(newType, context, asEdmEntity);
                    //TODO: should be added as transient entity. 
                    //I suspect that we have to implement this as well as I found no support for  transient entities in the current stack.
                    // AggregationTransientEntityAnnotation was created to mark the entity as transient.
                    model.AddElement(entityType);

                    if (asEdmEntity)
                    {
                        model.SetAnnotationValue(entityType,
                            new AggregationTransientEntityAnnotation() {EntityState = "TransientEntity"});
                    }
                }
                return newType;
            }
        }

        private static EdmComplexType CreateEdmComplexType(Type clrType, ODataQueryContext context)
        {
            Contract.Assert(clrType != null);
            Contract.Assert(context != null);

            var entryEdmType = new EdmComplexType("ODataAggregation.DynamicTypes", clrType.Name);
            foreach (var pi in clrType.GetProperties())
            {
                if (pi.Name == "ComparerInstance")
                {
                    continue;
                }
                if (pi.PropertyType.IsPrimitive || pi.PropertyType.FullName == "System.String")
                {
                    entryEdmType.AddStructuralProperty(
                        pi.Name,
                        GetPrimitiveTypeKind(pi.PropertyType),
                        true);
                }
                else
                {
                    var propEdmType = context.Model.FindDeclaredType(pi.PropertyType.FullName);
                    if (propEdmType != null)
                    {
                        entryEdmType.AddStructuralProperty(
                            pi.Name,
                            propEdmType.ToEdmTypeReference(true));
                    }
                }
            }
            return entryEdmType;
        }
        
        private static EdmEntityType CreateEdmEntityType(Type clrType, ODataQueryContext context)
        {
            Contract.Assert(clrType != null);
            Contract.Assert(context != null);
            var entryEdmType = new EdmEntityType("ODataAggregation.DynamicTypes", clrType.Name);

            foreach (var pi in clrType.GetProperties())
            {
                if (pi.Name == "ComparerInstance")
                {
                    continue;
                }
                if (pi.PropertyType.IsPrimitive || pi.PropertyType.FullName == "System.String" || pi.PropertyType.IsEnum)
                {
                    entryEdmType.AddStructuralProperty(
                        pi.Name,
                        GetPrimitiveTypeKind(pi.PropertyType),
                        true);
                }
                else
                {
                    var propEdmType = context.Model.FindDeclaredType(pi.PropertyType.FullName);
                    if (propEdmType != null)
                    {
                        entryEdmType.AddStructuralProperty(
                            pi.Name,
                            propEdmType.ToEdmTypeReference(true));
                    }
                }
            }
           
            return entryEdmType;
        }
        
        public static IEdmSchemaElement CreateEdmSechmaElement(Type clrType, ODataQueryContext context, bool asEdmEntity)
        {
            Contract.Assert(clrType != null);
            Contract.Assert(context != null);

            IEdmSchemaElement entryEdmType;
            if (asEdmEntity)
                return CreateEdmEntityType(clrType, context);

            return CreateEdmComplexType(clrType, context);
        }

        private static string GenerateClassName(List<Tuple<Type, string>> properties, bool asEdmEntity)
        {
            string prefix = asEdmEntity ? "Entity" : "ComplexType";
            properties.Sort((t1, t2) => string.Compare(t1.Item1.FullName, t2.Item1.FullName));
            StringBuilder valueToHash = new StringBuilder();
            foreach (var property in properties)
            {
                valueToHash.Append(string.Format("{0}-{1}", property.Item1, property.Item2));
            }
            return prefix + CalculateMD5Hash(valueToHash.ToString());
        }
        
        private static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return "DynamicType" + sb.ToString();
        }

        private static string CreateCode(string typeName, List<Tuple<Type, string>> properties)
        {
            string classTemplate = @"namespace ODataAggregation.DynamicTypes {{ public class {0} {{ {1} {2} }} }}";
            string propTemplate = @"public {0} {1} {{ get; set; }} ";
            StringBuilder propertiesCode = new StringBuilder();
            foreach (var property in properties)
            {
                propertiesCode.Append(string.Format(propTemplate, property.Item1.FullName, property.Item2));
            }
            string body = CreateEqualsMethods(typeName, properties.ToArray()) + CreateComparerProperty(typeName) +
                          CreateComparerClass(typeName);
            return string.Format(classTemplate, typeName, propertiesCode.ToString(), body);
        }

        private static string CreateEqualsMethods(string typeName, params Tuple<Type, string>[] properties)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format(@"public override bool Equals(object obj) {{ if (obj == null) return false; if (obj is {0}) {{  ", typeName));
            for (int i = 0; i < properties.Length; i++)
            {
                sb.Append(string.Format(@"if ((({0})obj).{1} == default({2}) && this.{1} != default({2})) {{ return false; }}", typeName, properties[i].Item2, properties[i].Item1));
                sb.Append(string.Format(@"if ((({0})obj).{1} != default({2}) && this.{1} == default({2})) {{ return false; }}", typeName, properties[i].Item2, properties[i].Item1));
            }
            sb.Append("return ");
            sb.Append(string.Format(@"((({0})obj).{1} == default({2}) && (this.{1} == default({2})) ||", typeName, properties[0].Item2, properties[0].Item1));
            sb.Append(string.Format(@"(({0})obj).{1}.Equals(this.{1}))", typeName, properties[0].Item2));
            for (int i = 1; i < properties.Length; i++)
            {
                sb.Append(string.Format(@"&&  ((({0})obj).{1} == default({2}) && (this.{1} == default({2})) || ", typeName, properties[i].Item2, properties[i].Item1));
                sb.Append(string.Format(@"(({0})obj).{1}.Equals(this.{1}))", typeName, properties[i].Item2));
            }
            sb.Append(";} else {  return false; } } ");
            sb.Append(@"public override int GetHashCode() { return ");
            sb.Append(string.Format(@"((this.{0} != default({1})) ? this.{0}.GetHashCode() : 0) ", properties[0].Item2, properties[0].Item1));
            for (int i = 1; i < properties.Length; i++)
            {
                sb.Append(string.Format(@" + ((this.{0} != default({1})) ? this.{0}.GetHashCode() : 0) ", properties[i].Item2, properties[i].Item1));
            }
            sb.Append(";}");

            return sb.ToString();

        }

        private static string CreateComparerProperty(string typeName)
        {
            return
                string.Format(
                    @"public static System.Collections.Generic.IEqualityComparer<{0}> ComparerInstance {{ get {{ return new {0}.Comparer(); }}}}",
                    typeName);
        }

        private static string CreateComparerClass(string typeName)
        {
            return string.Format(@"public class Comparer : System.Collections.Generic.IEqualityComparer<{0}>{{ public bool Equals({0} x, {0} y) {{  if ((x == null) && (y == null)){{ return true; }}  if ((x == null) || (y == null)) {{ return false; }}  return x.Equals(y); }} public int GetHashCode({0} obj) {{ return obj.GetHashCode(); }} }}", typeName);
        }
        
        private static EdmPrimitiveTypeKind GetPrimitiveTypeKind(Type t)
        {
            Contract.Assert(t != null);
            switch (t.Name)
            {
                case "Boolean": return EdmPrimitiveTypeKind.Boolean;
                case "Byte": return EdmPrimitiveTypeKind.Byte;
                case "DateTime": return EdmPrimitiveTypeKind.DateTimeOffset;
                case "DateTimeOffset": return EdmPrimitiveTypeKind.DateTimeOffset;
                case "Decimal": return EdmPrimitiveTypeKind.Decimal;
                case "Double": return EdmPrimitiveTypeKind.Double;
                case "Int16": return EdmPrimitiveTypeKind.Int16;
                case "Int32": return EdmPrimitiveTypeKind.Int32;
                case "Int64": return EdmPrimitiveTypeKind.Int64;
                case "SByte": return EdmPrimitiveTypeKind.SByte;
                case "String": return EdmPrimitiveTypeKind.String;
                case "Single": return EdmPrimitiveTypeKind.Single;
                case "Guid": return EdmPrimitiveTypeKind.Guid;
                case "Duration": return EdmPrimitiveTypeKind.Duration;
            }

            if (t.IsEnum)
            {
                return EdmPrimitiveTypeKind.Int32;
            }

            throw Error.InvalidOperation("unsupported type");
        }
    }
}
