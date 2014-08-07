// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.OData.Formatter;
using System.Web.OData.Query.Expressions;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;

namespace System.Web.OData
{
    internal static class ExpressionHelpers
    {

        public static object Count(Type t, IQueryable query)
        {
            MethodInfo maxMethod = ExpressionHelperMethods.QueryableSimpleCountGeneric.MakeGenericMethod(t);
            return maxMethod.Invoke(null, new[] { query });
        }

        public static object Max(Type t, IQueryable query)
        {
            MethodInfo maxMethod = ExpressionHelperMethods.QueryableMaxGeneric.MakeGenericMethod(t);
            return maxMethod.Invoke(null, new[] { query });
        }


        public static object Min(Type t, IQueryable query)
        {
            MethodInfo minMethod = ExpressionHelperMethods.QueryableMinGeneric.MakeGenericMethod(t);
            return minMethod.Invoke(null, new[] { query });
        }
        

        public static IQueryable Distinct(Type t, IQueryable query)
        {
            MethodInfo distinctMethod = ExpressionHelperMethods.QueryableDistinctGeneric.MakeGenericMethod(t);
            return distinctMethod.Invoke(null, new[] { query }) as IQueryable;
        }

        public static IQueryable AsQueryable(Type t, IEnumerable query)
        {
            MethodInfo asQueryableMethod = ExpressionHelperMethods.EnumerabltAsQueriableGeneric.MakeGenericMethod(t);
            return asQueryableMethod.Invoke(null, new[] { query }) as IQueryable;
        }

        public static IEnumerable Select(Type groupedItemType, Type resType, IQueryable dataToProject, LambdaExpression selector)
        {
            MethodInfo selectMethod = ExpressionHelperMethods.QueryableSelectGeneric.MakeGenericMethod(groupedItemType, resType);
            return selectMethod.Invoke(null, new Object[] { dataToProject, selector }) as IEnumerable;
        }

        public static IQueryable Cast(Type t, IQueryable query)
        {
            MethodInfo castMethod = ExpressionHelperMethods.QueryableCastByGeneric.MakeGenericMethod(t);
            return castMethod.Invoke(null, new[] { query }) as IQueryable;
        }

        public static object Aggregate<T>(IQueryable query, Expression<Func<T, T, T>> aggregationFunc)
        {
            MethodInfo aggregateMethod = ExpressionHelperMethods.QueryableGroupByGeneric.MakeGenericMethod(typeof(T));
            return aggregateMethod.Invoke(null, new object[] { query, aggregationFunc });
        }

        public static double Aggregate(IQueryable query, Expression<Func<double, double, double>> aggregationFunc)
        {
            MethodInfo aggregateMethod = ExpressionHelperMethods.QueryableAggregateGeneric.MakeGenericMethod(typeof(double));
            return (double)aggregateMethod.Invoke(null, new object[] { query, aggregationFunc });
        }


        public static IQueryable GroupBy(IQueryable query, Expression keySelector, Type itemType, Type keyType, object comparer)
        {
            MethodInfo groupbyMethod = ExpressionHelperMethods.QueryableGroupByGeneric.MakeGenericMethod(itemType, keyType);
            return groupbyMethod.Invoke(null, new object[] { query, keySelector, comparer }) as IQueryable;
        }


        public static IEnumerable Select(IEnumerable enumerable, Type elementType, Type resultType, LambdaExpression projectionLambda)
        {
            MethodInfo selectMethod = ExpressionHelperMethods.EnumerableSelectGeneric.MakeGenericMethod(elementType, resultType);
            return selectMethod.Invoke(null, new object[] { enumerable, projectionLambda.Compile() }) as IEnumerable;
        }


        public static object SelectAndSum(IQueryable query, Type elementType, Type resultType, LambdaExpression projectionLambda)
        {
            MethodInfo selectAndSumMethod = ExpressionHelperMethods.QueryableSelectAndSumGeneric(resultType).MakeGenericMethod(elementType);
            return selectAndSumMethod.Invoke(null, new object[] { query, projectionLambda });
        }

        public static object SelectAndAverage(IQueryable query, Type elementType, Type resultType, LambdaExpression projectionLambda)
        {
            MethodInfo selectAndSumMethod = ExpressionHelperMethods.QueryableSelectAndAverageGeneric(resultType).MakeGenericMethod(elementType);
            return selectAndSumMethod.Invoke(null, new object[] { query, projectionLambda });
        }

        public static long Count(IQueryable query, Type type)
        {
            MethodInfo countMethod = ExpressionHelperMethods.QueryableCountGeneric.MakeGenericMethod(type);
            return (long)countMethod.Invoke(null, new object[] { query });
        }

        public static IQueryable Skip(IQueryable query, int count, Type type, bool parameterize)
        {
            MethodInfo skipMethod = ExpressionHelperMethods.QueryableSkipGeneric.MakeGenericMethod(type);
            Expression skipValueExpression = parameterize ? LinqParameterContainer.Parameterize(typeof(int), count) : Expression.Constant(count);

            Expression skipQuery = Expression.Call(null, skipMethod, new[] { query.Expression, skipValueExpression });
            return query.Provider.CreateQuery(skipQuery);
        }

        public static IQueryable Take(IQueryable query, int count, Type type, bool parameterize)
        {
            Expression takeQuery = Take(query.Expression, count, type, parameterize);
            return query.Provider.CreateQuery(takeQuery);
        }

        public static Expression Take(Expression source, int count, Type elementType, bool parameterize)
        {
            MethodInfo takeMethod;
            if (typeof(IQueryable).IsAssignableFrom(source.Type))
            {
                takeMethod = ExpressionHelperMethods.QueryableTakeGeneric.MakeGenericMethod(elementType);
            }
            else
            {
                takeMethod = ExpressionHelperMethods.EnumerableTakeGeneric.MakeGenericMethod(elementType);
            }

            Expression takeValueExpression = parameterize ? LinqParameterContainer.Parameterize(typeof(int), count) : Expression.Constant(count);
            Expression takeQuery = Expression.Call(null, takeMethod, new[] { source, takeValueExpression });
            return takeQuery;
        }

        public static IQueryable OrderByIt(IQueryable query, OrderByDirection direction, Type type, bool alreadyOrdered = false)
        {
            ParameterExpression odataItParameter = Expression.Parameter(type, "$it");
            LambdaExpression orderByLambda = Expression.Lambda(odataItParameter, odataItParameter);
            return OrderBy(query, orderByLambda, direction, type, alreadyOrdered);
        }

        public static IQueryable OrderByProperty(IQueryable query, IEdmModel model, IEdmProperty property, OrderByDirection direction, Type type, bool alreadyOrdered = false)
        {
            // property aliasing
            string propertyName = EdmLibHelpers.GetClrPropertyName(property, model);
            LambdaExpression orderByLambda = GetPropertyAccessLambda(type, propertyName);
            return OrderBy(query, orderByLambda, direction, type, alreadyOrdered);
        }

        public static IQueryable OrderBy(IQueryable query, LambdaExpression orderByLambda, OrderByDirection direction, Type type, bool alreadyOrdered = false)
        {
            Type returnType = orderByLambda.Body.Type;

            MethodInfo orderByMethod = null;
            IOrderedQueryable orderedQuery = null;

            // unfortunately unordered L2O.AsQueryable implements IOrderedQueryable
            // so we can't try casting to IOrderedQueryable to provide a clue to whether
            // we should be calling ThenBy or ThenByDescending
            if (alreadyOrdered)
            {
                if (direction == OrderByDirection.Ascending)
                {
                    orderByMethod = ExpressionHelperMethods.QueryableThenByGeneric.MakeGenericMethod(type, returnType);
                }
                else
                {
                    orderByMethod = ExpressionHelperMethods.QueryableThenByDescendingGeneric.MakeGenericMethod(type, returnType);
                }

                orderedQuery = query as IOrderedQueryable;
                orderedQuery = orderByMethod.Invoke(null, new object[] { orderedQuery, orderByLambda }) as IOrderedQueryable;
            }
            else
            {
                if (direction == OrderByDirection.Ascending)
                {
                    orderByMethod = ExpressionHelperMethods.QueryableOrderByGeneric.MakeGenericMethod(type, returnType);
                }
                else
                {
                    orderByMethod = ExpressionHelperMethods.QueryableOrderByDescendingGeneric.MakeGenericMethod(type, returnType);
                }

                orderedQuery = orderByMethod.Invoke(null, new object[] { query, orderByLambda }) as IOrderedQueryable;
            }

            return orderedQuery;
        }

        public static IQueryable Where(IQueryable query, Expression where, Type type)
        {
            MethodInfo whereMethod = ExpressionHelperMethods.QueryableWhereGeneric.MakeGenericMethod(type);
            return whereMethod.Invoke(null, new object[] { query, where }) as IQueryable;
        }

        // If the expression is not a nullable type, cast it to one.
        public static Expression ToNullable(Expression expression)
        {
            if (!expression.Type.IsNullable())
            {
                return Expression.Convert(expression, expression.Type.ToNullable());
            }

            return expression;
        }

        // Entity Framework does not understand default(T) expression. Hence, generate a constant expression with the default value.
        public static Expression Default(Type type)
        {
            if (type.IsValueType)
            {
                return Expression.Constant(Activator.CreateInstance(type), type);
            }
            else
            {
                return Expression.Constant(null, type);
            }
        }

        private static LambdaExpression GetPropertyAccessLambda(Type type, string propertyName)
        {
            ParameterExpression odataItParameter = Expression.Parameter(type, "$it");
            MemberExpression propertyAccess = Expression.Property(odataItParameter, propertyName);
            return Expression.Lambda(propertyAccess, odataItParameter);
        }
    }
}
