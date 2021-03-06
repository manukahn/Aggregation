﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.OData.Core.UriParser.Semantic;

namespace System.Web.OData.OData.Query.Aggregation
{
    /// <summary>
    /// Base type for implementation class of aggregation methods
    /// </summary>
    public abstract class AggregationImplementationBase : ApplyImplementationBase
    {
        /// <summary>
        /// Collection of delegates for obtaining a property value of the entity to aggregate
        /// </summary>
        protected static ConcurrentDictionary<string, Delegate> propertyProjectionDelegates = new ConcurrentDictionary<string, Delegate>();

        /// <summary>
        /// Execute the aggregation method.
        /// </summary>
        /// <param name="elementType">The element type on which the aggregation method operates.</param>
        /// <param name="collection">The collection on which to execute.</param>
        /// <param name="transformation">The name of the aggregation transformation.</param>
        /// <param name="propertyToAggregateExpression">Expression to the property to aggregate.</param>
        /// <param name="parameters">A list of string parameters sent to the aggregation method</param>
        /// <returns>The result of the aggregation.</returns>
        public abstract object DoAggregatinon(Type elementType, IQueryable collection, ApplyAggregateClause transformation, LambdaExpression propertyToAggregateExpression, params string[] parameters);

        /// <summary>
        /// Determines the type that is returned from the aggregation method. 
        /// </summary>
        /// <param name="elementType">The element type on which the aggregation method operates.</param>
        /// <param name="transformation">The name of the aggregation transformation.</param>
        /// <returns>The type that is returned from the aggregation method.</returns>
        public abstract Type GetResultType(Type elementType, ApplyAggregateClause transformation);


        /// <summary>
        /// Combine temporary results into one final result.
        /// </summary>
        /// <param name="temporaryResults">The results to combine, as <see cref="Tuple{object, int}"/> when item1 is the result. 
        /// and item2 is the number of elements that produced this temporary result.</param>
        /// <returns>The final result.</returns>
        public abstract object CombineTemporaryResults(List<Tuple<object, int>> temporaryResults);


        /// <summary>
        /// Get the delegates for obtaining a property value of the entity to aggregate
        /// </summary>
        /// <param name="elementType">The entity Type</param>
        /// <param name="aggregatableProperty">The property to aggregate</param>
        /// <param name="propertyToAggregateExpression">The expression to use for obtaining the property value</param>
        /// <returns>A delegate for obtaining a property value of the entity to aggregate</returns>
        protected static Delegate GetProjectionDelegate(Type elementType, string aggregatableProperty, LambdaExpression propertyToAggregateExpression)
        {
            Delegate projectionDelegate;
            var projectionIdentifier = string.Format("{0}.{1}.{2}",elementType.Namespace, elementType.Name, aggregatableProperty);
            if (!propertyProjectionDelegates.TryGetValue(projectionIdentifier, out projectionDelegate))
            {
                projectionDelegate = propertyToAggregateExpression.Compile();
                propertyProjectionDelegates[projectionIdentifier] = projectionDelegate;
            }
            return projectionDelegate;
        }


        /// <summary>
        /// Helper method to get the type of a property path such as Sales.Product.Category.Name. 
        /// This method returns the type of the Name property.
        /// </summary>
        /// <param name="entityType">The type to investigate.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The type of the property to aggregate.</returns>
        protected Type GetAggregatedPropertyType(Type entityType, string propertyPath)
        {
            Contract.Assert(!String.IsNullOrEmpty(propertyPath));
            Contract.Assert(entityType != null);

            var propertyInfo = GetPropertyInfo(entityType, propertyPath);
            if (propertyInfo != null && propertyInfo.Count > 0)
            {
                return propertyInfo.Last().PropertyType;
            }

            return null;
        }
        
        /// <summary>
        /// Get the selected values to aggregate
        /// </summary>
        /// <param name="elementType">The type of entities</param>
        /// <param name="collection">The collection to aggregate</param>
        /// <param name="transformation">The transformation clause created by the parser</param>
        /// <param name="propertyToAggregateExpression">Projection Expression that defines access to the property to aggregate</param>
        /// <returns>The selected values to aggregate</returns>
        protected IQueryable GetSelectedValues(Type elementType, IQueryable collection, ApplyAggregateClause transformation,
            LambdaExpression propertyToAggregateExpression)
        {
            var aggregatedProperyType = this.GetAggregatedPropertyType(elementType, transformation.AggregatableProperty);
            var projectionDelegate = GetProjectionDelegate(elementType, transformation.AggregatableProperty, propertyToAggregateExpression);
            return GetItemsToQuery(elementType, collection, projectionDelegate, aggregatedProperyType);
        }

        /// <summary>
        /// Create a projection lambda if one was not provided.
        /// </summary>
        /// <param name="elementType">The type of entities.</param>
        /// <param name="transformation">The transformation clause created by the parser.</param>
        /// <param name="propertyToAggregateExpression">Projection Expression to that defines access to the property to aggregate.</param>
        /// <returns>A lambda expression to the property to aggregate.</returns>
        public static LambdaExpression GetProjectionLambda(Type elementType, ApplyAggregateClause transformation,
            LambdaExpression propertyToAggregateExpression)
        {
            LambdaExpression projectionLambda;
            if (propertyToAggregateExpression == null)
            {
                projectionLambda = GetProjectionLambda(elementType, transformation.AggregatableProperty);
            }
            else
            {
                projectionLambda = propertyToAggregateExpression as LambdaExpression;
            }
            return projectionLambda;
        }


        /// <summary>
        /// Create an expression that validate that a path to a property does not contain null values
        /// For example: for the query product/category/name, create expression such as x.product != null && x.product.Category != null.
        /// </summary>
        /// <param name="elementType">The element type.</param>
        /// <param name="transformation">The aggregation transformation.</param>
        /// <returns>The result with null propagation checks.</returns>
        public static IQueryable FilterNullValues(IQueryable query, Type elementType, ApplyAggregateClause transformation)
        {
            var entityParam = Expression.Parameter(elementType, "e");
            IQueryable queryToUse = query;

            var accessorExpressions = GetProjectionExpressions(transformation.AggregatableProperty, entityParam).ToArray();
            var accessorExpressionsToUse = new List<Expression>();
            for (int i = 0; i < accessorExpressions.Length - 1; i++)
            {
                accessorExpressionsToUse.Add(
                    Expression.Lambda(Expression.MakeBinary(ExpressionType.NotEqual, accessorExpressions[i], Expression.Constant(null)),
                    entityParam));
            }

            foreach (var exp in accessorExpressionsToUse)
            {
                queryToUse = ExpressionHelpers.Where(queryToUse, exp, elementType);
            }
            return queryToUse;
        }

        /// <summary>
        /// Create a queryable of items to process.
        /// </summary>
        /// <param name="elementType">Type of elements in input queryable.</param>
        /// <param name="query">input queryable</param>
        /// <param name="propertyToAggregateExpression">The lambda expression that chooses a property from the input elements.</param>
        /// <param name="aggregatedPropertyType">Type of the selected property to aggregate.</param>
        /// <returns>The collection of items to query.</returns>
        public static IQueryable GetItemsToQuery(Type elementType, IQueryable query, Delegate propertyToAggregateExpression, Type aggregatedPropertyType)
        {
            var selected = (ExpressionHelpers.Select(query, elementType, aggregatedPropertyType, propertyToAggregateExpression)).AsQueryable();
            return selected;
        }

        /// <summary>
        /// Parse a list of arguments from the method string 
        /// </summary>
        /// <param name="aggragationMethod">The method string</param>
        /// <returns>List of arguments</returns>
        internal static string[] GetAggregationParams(string aggragationMethod)
        {
            if (aggragationMethod.Contains('(') && aggragationMethod.Contains(')'))
            {
                var start = aggragationMethod.LastIndexOf('(');
                var end = aggragationMethod.IndexOf(')');
                if (end == start + 1)
                {
                    return null;
                }
                if (end <= start)
                {
                    throw new ArgumentException("Invalid parameters string");
                }

                return aggragationMethod.Substring(start + 1, end - start - 1).Split(',');
            }

            return null;
        }
    }
}
