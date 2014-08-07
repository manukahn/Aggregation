using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.OData.Core.UriParser.Semantic;

namespace System.Web.OData.OData.Query.Aggregation.AggregationMethods
{
    /// <summary>
    /// Implementation of Sum aggregation method
    /// </summary>
    public class SumAggregation : AggregationImplementationBase
    {
        /// <summary>
        /// Implement the Sum aggregation method
        /// </summary>
        /// <param name="elementType">The type of entities</param>
        /// <param name="query">The collection</param>
        /// <param name="transformation">The transformation clause created by the parser</param>
        /// <param name="propertyToAggregateExpression">Projection Expression that defines access to the property to aggregate</param>
        /// <returns>The Sum result</returns>
        public override object DoAggregatinon(Type elementType, IQueryable query, ApplyAggregateClause transformation, LambdaExpression propertyToAggregateExpression)
        {
            IQueryable queryToUse = query;
            if (transformation.AggregatableProperty.Contains('/'))
            {
                queryToUse = FilterNullValues(query, elementType, transformation);
            }

            var projectionLambda = GetProjectionLambda(elementType, transformation, propertyToAggregateExpression);
            var resultType = GetResultType(elementType, transformation);
            return ExpressionHelpers.SelectAndSum(queryToUse, elementType, resultType, projectionLambda);
        }

        /// <summary>
        /// Get the type of the aggregation result
        /// </summary>
        /// <param name="elementType">the entity type</param>
        /// <param name="transformation">The transformation clause created by the parser</param>
        /// <returns>The type of the aggregation result</returns>
        public override Type GetResultType(Type elementType, ApplyAggregateClause transformation)
        {
            return GetAggregatedPropertyType(elementType, transformation.AggregatableProperty);
        }
    }
}
