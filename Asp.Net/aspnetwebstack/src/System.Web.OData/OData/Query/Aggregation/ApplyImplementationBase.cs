using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData.Formatter;
using Microsoft.OData.Core;
using Microsoft.OData.Core.Aggregation;
using Microsoft.OData.Core.UriParser.Semantic;

namespace System.Web.OData.OData.Query.Aggregation
{
    /// <summary>
    /// Base class for implementation classes.
    /// </summary>
    public abstract class ApplyImplementationBase
    {
        /// <summary>
        /// Create the Expression that defines a projection statement such as s.Amount;
        /// </summary>
        /// <param name="propertyPath">The property path to project.</param>
        /// <param name="entityParam">The entity param that contains the entity to project.</param>
        /// <returns>An <see cref="Expression"/> that defines a projection statement.</returns>
        internal static Expression GetProjectionExpression(string propertyPath, ParameterExpression entityParam)
        {
            Contract.Assert(entityParam != null);
            Contract.Assert(!string.IsNullOrEmpty(propertyPath));

            var propertyInfos = GetPropertyInfo(entityParam.Type, propertyPath);
            Expression body = entityParam;
            foreach (var pi in propertyInfos)
            {
                body = Expression.Property(body, (PropertyInfo)pi);
            }

            return body;
        }

        /// <summary>
        /// Get a <see cref="MemberExpression"/> that defines all the projections that should be used to access the property all the way from the root object.
        /// </summary>
        /// <param name="propertyPath">The path to the property.</param>
        /// <param name="entityParam">The root object.</param>
        /// <returns>A <see cref="List{Expression}"/>that defines all the projection statements that will be used to access a property from the root object.</returns>
        internal static List<Expression> GetProjectionExpressions(string propertyPath, ParameterExpression entityParam)
        {
            Contract.Assert(entityParam != null);
            Contract.Assert(!string.IsNullOrEmpty(propertyPath));
            var res = new List<Expression>();

            var propertyInfos = GetPropertyInfo(entityParam.Type, propertyPath);
            Expression body = entityParam;
            foreach (var pi in propertyInfos)
            {
                body = Expression.Property(body, (PropertyInfo)pi);
                res.Add(body);
            }

            return res;
        }


        /// <summary>
        /// Create a LambdaExpression expression that defines a projection statement such as: Expression{Func{Sales, double}} projectionLambda = s => s.Amount;
        /// </summary>
        /// <param name="entityType">The entity type to from which to project.</param>
        /// <param name="propertyPath">The property path to project.</param>
        /// <returns>LambdaExpression expression that defines a projection statement.</returns>
        internal static LambdaExpression GetProjectionLambda(Type entityType, string propertyPath)
        {
            var entityParam = Expression.Parameter(entityType, "e");
            Expression body = GetProjectionExpression(propertyPath, entityParam);
            return Expression.Lambda(body, entityParam);
        }

        /// <summary>
        /// create an expression of a call to a static method such as: Expression{Func{Sales, double}} <code>Lambda = s => s.Method();</code>.
        /// </summary>
        /// <param name="entityType">The entity type to from which to project.</param>
        /// <param name="method">The method to call.</param>
        /// <returns>LambdaExpression expression that defines a method call statement.</returns>
        internal static LambdaExpression GetMethodCallLambda(Type entityType, MethodInfo method)
        {
            var entityParam = Expression.Parameter(entityType, "e");
            Expression body = Expression.Call(null, method, entityParam);
            return Expression.Lambda(body, entityParam);
        }


        /// <summary>
        /// Generate an expression to retrieve a property value from an object.
        /// if propertyPath is a simple property such as: Name, return a simple projection expression such as: e.Name. 
        /// if propertyPath is a navigation path (such as Product/Category/Name) represent the expression for: 
        /// new Product(){Category = new Category(){Name = e.Product.Category.Name}.
        /// </summary>
        /// <param name="entityType">The type of the object on which projection is done.</param>
        /// <param name="propertyPath">The property path of the property to retrieve.</param>
        /// <param name="entityParam">The parameter on which the expression is based.</param>
        /// <param name="selectedProperyExpression">Expression created by OData parser.</param>
        /// <param name="originalPropertyPath">The property path of the property to retrieve that was used originally used (out side recursion).</param>
        /// <returns>An Expression that defines projection.</returns>
        internal static Expression GetPropertyExpression(Type entityType, string propertyPath, ParameterExpression entityParam, Expression selectedProperyExpression, string originalPropertyPath = null)
        {
            if (originalPropertyPath == null)
            {
                originalPropertyPath = propertyPath;
            }

            if (!propertyPath.Contains('/'))
            {
                return (selectedProperyExpression ?? GetProjectionExpression(originalPropertyPath, entityParam));
            }
            else
            {
                var propertyInfos = GetPropertyInfo(entityType, propertyPath);
                var entityPropertyInfo = GetPropertyInfo(entityParam.Type, originalPropertyPath);
                var pi = propertyInfos[0];
                var mi = propertyInfos[1];
                var entityPi = entityPropertyInfo.First(item => item.Name == pi.Name);
                var entityPropertyExpression = GetProjectionExpressions(originalPropertyPath, entityParam).First(item => item.Type == entityPi.PropertyType);

                var newInstance = Expression.New(pi.PropertyType.GetConstructors().First());
                var propertyExpresssion = GetPropertyExpression(
                    mi.DeclaringType,
                    propertyPath.Substring(propertyPath.IndexOf('/') + 1),
                    entityParam, selectedProperyExpression, originalPropertyPath);

                var binding = Expression.Bind(mi, propertyExpresssion);

                try
                {
                    // Handle null propagation
                    var testExpression = Expression.MakeBinary(ExpressionType.Equal, entityPropertyExpression, Expression.Constant(null));
                    var whenFalse = Expression.MemberInit(newInstance, new MemberBinding[] { binding });
                    var whenTrue = Expression.Constant(null, pi.PropertyType);
                    var result = Expression.Condition(testExpression, whenTrue, whenFalse, pi.PropertyType);
                    return result;
                }
                catch (InvalidOperationException)
                {
                    // types like dateTimeOffset will throw : The binary operator Equal is not defined for the types 'System.DateTimeOffset' and 'System.Object'.
                    return Expression.MemberInit(newInstance, new MemberBinding[] { binding });
                }
            }
        }


        /// <summary>
        /// Generate an expression to retrieve a computed property by executing a method on one of the object properties.
        /// if propertyPath is a simple property such as: Name, return a methodCall expression simple projection expression such as: method(e.Name). 
        /// if propertyPath is a navigation path (such as Product/Category/Name) represent the expression for: 
        /// new Product(){Category = new Category(){alias = method(e.Product.Category.Name)}.
        /// </summary>
        /// <param name="entityType">The type of the object on which projection is done.</param>
        /// <param name="propertyPath">The property path of the property to retrieve.</param>
        /// <param name="entityParam">The parameter on which the expression is based.</param>
        /// <param name="methodToExecute">The method to call.</param>
        /// <param name="selectedProperyExpression">An expression representing access to the property to group by.</param>
        /// <param name="samplingParametersExpressions">A list of parameters to the sampling method</param>
        /// <param name="originalPropertyPath">The property path of the property to retrieve that was used originally used (out side recursion).</param>
        /// <returns>An Expression that defines projection.</returns>
        internal static Expression GetComputedPropertyExpression(Type entityType, string propertyPath,
            ParameterExpression entityParam, MethodInfo methodToExecute, Expression selectedProperyExpression, Expression[] samplingParametersExpressions, string originalPropertyPath = null)
        {
            if (originalPropertyPath == null)
            {
                originalPropertyPath = propertyPath;
            }

            if (!propertyPath.Contains('/'))
            {
                Expression[] arguments;

                var selectedProperty = (selectedProperyExpression != null)
                    ? selectedProperyExpression
                    : GetProjectionExpression(originalPropertyPath, entityParam);

                if ((samplingParametersExpressions == null) || (!samplingParametersExpressions.Any()))
                {
                    arguments = new Expression[] { selectedProperty };
                }
                else
                {
                    var lst = new List<Expression>() { selectedProperty };
                    lst.AddRange(samplingParametersExpressions);
                    arguments = lst.ToArray();
                }

                return Expression.Call(null, methodToExecute, arguments);
            }

            var propertyInfos = GetPropertyInfo(entityType, propertyPath);
            var pi = propertyInfos[0];
            var entityPropertyInfo = GetPropertyInfo(entityParam.Type, originalPropertyPath);
            var computedProperty = propertyInfos[1];
            var entityPi = entityPropertyInfo.First(item => item.Name == pi.Name);
            var entityPropertyExpression = GetProjectionExpressions(originalPropertyPath, entityParam).First(item => item.Type == entityPi.PropertyType);

            var newInstance = Expression.New(pi.PropertyType.GetConstructors().First());

            var propertyExpression = GetComputedPropertyExpression(
                computedProperty.DeclaringType,
                propertyPath.Substring(propertyPath.IndexOf('/') + 1),
                entityParam,
                methodToExecute,
                selectedProperyExpression,
                samplingParametersExpressions,
                originalPropertyPath);

            var binding = Expression.Bind(computedProperty, propertyExpression);

            // Handle null propagation
            var testExpression = Expression.MakeBinary(ExpressionType.Equal, entityPropertyExpression, Expression.Constant(null));
            var whenFalse = Expression.MemberInit(newInstance, new MemberBinding[] { binding });
            var whenTrue = Expression.Constant(null, pi.PropertyType);
            var result = Expression.Condition(testExpression, whenTrue, whenFalse, pi.PropertyType);
            return result;
        }


        /// <summary>
        /// Returns a list of PropertyInfo describing a path to a property in a given type;
        /// For example Product/Category/Name will result three property info objects: Product, Category and Name.
        /// Supports also path statements that contain alias names such as "Amount with sum as Total".
        /// </summary>
        /// <param name="t">Type to explore.</param>
        /// <param name="path">Property path.</param>
        /// <returns>A list of property info object that describe a path to the object.</returns>
        internal static List<PropertyInfo> GetPropertyInfo(Type t, string path)
        {
            var real = t.GetProperty(path.Split(' ').First().TrimMethodCallPrefix());
            var alias = t.GetProperty(path.Split(' ').Last().TrimMethodCallSufix());
            if (real != null)
            {
                return new List<PropertyInfo>() { real };
            }

            if (alias != null)
            {
                return new List<PropertyInfo>() { alias };
            }

            string[] segments = path.Split('/');
            if (segments.Length <= 1)
            {
                return null;
            }

            var tmp = t.GetProperty(segments[0].Split(' ').First().TrimMethodCallPrefix());
            if (tmp == null)
            {
                throw Error.InvalidOperation("Invalid Path {0}", path);
            }

            var res = new List<PropertyInfo>() { tmp };
            res.AddRange(GetPropertyInfo(tmp.PropertyType, path.Substring(segments[0].Length + 1)));
            return res;
        }
    }
}
