using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
        private static ConcurrentDictionary<string, object> syncLocks = new ConcurrentDictionary<string, object>();
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
            return syncLocks.GetOrAdd(key, _ => new object());
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
        /// <returns>The new generated type</returns>
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

        /// <summary>
        /// Create a entry in the EDM document for this new type 
        /// </summary>
        /// <param name="clrType">The new type that was generated</param>
        /// <param name="context">The OData context</param>
        /// <param name="asEdmEntity">Determines whether to create an entity or a complex type</param>
        /// <returns>The new EDM schema element as a <see cref="IEdmSchemaElement"/></returns>
        public static IEdmSchemaElement CreateEdmSechmaElement(Type clrType, ODataQueryContext context, bool asEdmEntity)
        {
            Contract.Assert(clrType != null);
            Contract.Assert(context != null);

            EdmStructuredType entryEdmType;
            if (asEdmEntity)
            {
                entryEdmType = new EdmEntityType("ODataAggregation.DynamicTypes", clrType.Name);
                CreateSchemaType(clrType, context, entryEdmType);
                return entryEdmType as IEdmSchemaElement;
            }

            entryEdmType = new EdmComplexType("ODataAggregation.DynamicTypes", clrType.Name);
            CreateSchemaType(clrType, context, entryEdmType);
            return entryEdmType as IEdmSchemaElement; 
        }
       

        /// <summary>
        /// Implement the creation of an EDM Entity Type or EDM Complex Type
        /// </summary>
        /// <param name="clrType">The CLR type of the new schema element to create</param>
        /// <param name="context">The query context</param>
        /// <param name="entryEdmType">The new Schema element</param>
        private static void CreateSchemaType(Type clrType, ODataQueryContext context, EdmStructuredType entryEdmType)
        {
            foreach (var pi in clrType.GetProperties())
            {
                if (pi.Name == "ComparerInstance")
                {
                    continue;
                }
                if (pi.PropertyType.IsPrimitive || pi.PropertyType.FullName == "System.String")
                {
                    entryEdmType.AddStructuralProperty(pi.Name, GetPrimitiveTypeKind(pi.PropertyType), true);
                }
                else if (pi.PropertyType.IsEnum)
                {
                    var enumType = GetEnumTypeKind(pi.PropertyType, context);
                    if (enumType != null)
                    {
                        entryEdmType.AddStructuralProperty(pi.Name, enumType);
                    }
                    else
                    {
                        entryEdmType.AddStructuralProperty(pi.Name, EdmPrimitiveTypeKind.Int32);
                    }
                }
                else
                {
                    var propEdmType = context.Model.FindDeclaredType(pi.PropertyType.FullName);
                    if (propEdmType != null)
                    {
                        entryEdmType.AddStructuralProperty(pi.Name, propEdmType.ToEdmTypeReference(true));
                    }
                    else
                    {
                        var t = Type.GetType(pi.PropertyType.FullName);
                        var edmType = context.Model.GetEdmTypeReference(t);
                        if (edmType != null)
                        {
                            entryEdmType.AddStructuralProperty(pi.Name, edmType);
                        }
                        else
                        {
                            throw new InvalidOperationException(string.Format("Could not find EDM type of {0}",pi.PropertyType.FullName));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Compute the class name that should be created. A class name contains a hash of the combination of its properties.
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="asEdmEntity"></param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Calculate a MD5 hash 
        /// </summary>
        /// <param name="input">string to hash</param>
        /// <returns>Hash encoded as string</returns>
        private static string CalculateMD5Hash(string input)
        {
            byte[] hash;

            // step 1, calculate MD5 hash from input
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                hash = md5.ComputeHash(inputBytes);
            }

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return "DynamicType" + sb.ToString();
        }

        /// <summary>
        /// Create the code of the new type
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        private static string CreateCode(string typeName, List<Tuple<Type, string>> properties)
        {
            string classTemplate = @"namespace ODataAggregation.DynamicTypes {{ public class {0} {{ {1} {2} }} }}";
            string propTemplate = @"public {0} {1} {{ get; set; }} ";
            StringBuilder propertiesCode = new StringBuilder();
            foreach (var property in properties)
            {
                var fullName = property.Item1.FullName.Replace('+', '.');
                propertiesCode.Append(string.Format(propTemplate, fullName, property.Item2));
            }
            string body = CreateEqualsMethods(typeName, properties.ToArray()) + CreateComparerProperty(typeName) +
                          CreateComparerClass(typeName);
            return string.Format(classTemplate, typeName, propertiesCode.ToString(), body);
        }

        /// <summary>
        /// Compose an Equals and a GetHashCode for the new type
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        private static string CreateEqualsMethods(string typeName, params Tuple<Type, string>[] properties)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format(@"public override bool Equals(object obj) {{ if (obj == null) return false; if (obj is {0}) {{  ", typeName));
            for (int i = 0; i < properties.Length; i++)
            {
                var propTypeName = properties[i].Item1.FullName.Replace('+', '.');
                sb.Append(string.Format(@"if ((({0})obj).{1} == default({2}) && this.{1} != default({2})) {{ return false; }}", typeName, properties[i].Item2, propTypeName));
                sb.Append(string.Format(@"if ((({0})obj).{1} != default({2}) && this.{1} == default({2})) {{ return false; }}", typeName, properties[i].Item2, propTypeName));
            }
            sb.Append("return ");
            sb.Append(string.Format(@"((({0})obj).{1} == default({2}) && (this.{1} == default({2})) ||", typeName, properties[0].Item2, properties[0].Item1.FullName.Replace('+', '.')));
            sb.Append(string.Format(@"(({0})obj).{1}.Equals(this.{1}))", typeName, properties[0].Item2));
            for (int i = 1; i < properties.Length; i++)
            {
                var propTypeName = properties[i].Item1.FullName.Replace('+', '.');
                sb.Append(string.Format(@"&&  ((({0})obj).{1} == default({2}) && (this.{1} == default({2})) || ", typeName, properties[i].Item2, propTypeName));
                sb.Append(string.Format(@"(({0})obj).{1}.Equals(this.{1}))", typeName, properties[i].Item2));
            }
            sb.Append(";} else {  return false; } } ");
            sb.Append(@"public override int GetHashCode() { return ");
            sb.Append(string.Format(@"((this.{0} != default({1})) ? this.{0}.GetHashCode() : 0) ", properties[0].Item2, properties[0].Item1.FullName.Replace('+', '.')));
            for (int i = 1; i < properties.Length; i++)
            {
                var propTypeName = properties[i].Item1.FullName.Replace('+', '.');
                sb.Append(string.Format(@" + ((this.{0} != default({1})) ? this.{0}.GetHashCode() : 0) ", properties[i].Item2, propTypeName));
            }
            sb.Append(";}");

            return sb.ToString();

        }

        /// <summary>
        /// Add a property of type IEqualityComparer to the new type
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private static string CreateComparerProperty(string typeName)
        {
            return
                string.Format(
                    @"public static System.Collections.Generic.IEqualityComparer<{0}> ComparerInstance {{ get {{ return new {0}.Comparer(); }}}}",
                    typeName);
        }

        /// <summary>
        /// Compose an implementation of IEqualityComparer
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private static string CreateComparerClass(string typeName)
        {
            return string.Format(@"public class Comparer : System.Collections.Generic.IEqualityComparer<{0}>{{ public bool Equals({0} x, {0} y) {{  if ((x == null) && (y == null)){{ return true; }}  if ((x == null) || (y == null)) {{ return false; }}  return x.Equals(y); }} public int GetHashCode({0} obj) {{ return obj.GetHashCode(); }} }}", typeName);
        }
        
        /// <summary>
        /// Helper method that maps a primitive type to <see cref="EdmPrimitiveTypeKind"/>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
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

            throw Error.InvalidOperation("unsupported type");
        }

        /// <summary>
        /// Find the <see cref="IEdmTypeReference"/> of an <see cref="Enum"/> property
        /// </summary>
        /// <param name="type">the CLR type of the property</param>
        /// <param name="context">The query context</param>
        /// <returns></returns>
        private static IEdmTypeReference GetEnumTypeKind(Type type, ODataQueryContext context)
        {
            var res = context.Model.FindDeclaredType(type.FullName);
            if (res != null)
            {
                return res.ToEdmTypeReference(true);
            }

            var elementType = context.Model.FindDeclaredType(context.ElementType.FullTypeName());
            var enumProperties = elementType.ToEdmTypeReference(true)
                .AsStructured()
                .StructuralProperties()
                .Where(p => p.Type as EdmEnumTypeReference != null);


            if (enumProperties.Count() == 1)
            {
                return enumProperties.First().Type;
            }

            var sameNameEnumProps = enumProperties.Where(p => p.Name == type.Name);
            if (sameNameEnumProps.Count() == 1)
            {
                return sameNameEnumProps.First().Type;
            }

            return null;
        }
      
    }
}
