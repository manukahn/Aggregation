using System.Linq;
using System.Linq.Expressions;
using Microsoft.OData.Core.UriParser.Semantic;

namespace System.Web.OData.OData.Query.Aggregation.AggregationMethods
{
    /// <summary>
    /// Implementation of Average aggregation method
    /// </summary>
    public class AverageAggregation : AggregationImplementationBase
    {
        /// <summary>
        /// Implement the Average aggregation method
        /// </summary>
        /// <param name="elementType">The type of entities</param>
        /// <param name="query">The collection</param>
        /// <param name="transformation">The transformation clause created by the parser</param>
        /// <param name="propertyToAggregateExpression">Projection Expression that defines access to the property to aggregate</param>
        /// <returns>The aggregation result</returns>
        public override object DoAggregatinon(Type elementType, IQueryable query, ApplyAggregateClause transformation, LambdaExpression propertyToAggregateExpression)
        {
            IQueryable queryToUse = query;
            if (transformation.AggregatableProperty.Contains('/'))
            {
                queryToUse = FilterNullValues(query, elementType, transformation);
            }

            var projectionLambda = GetProjectionLambda(elementType, transformation, propertyToAggregateExpression);
            var resultType = GetResultType(elementType, transformation);
            return ExpressionHelpers.SelectAndAverage(queryToUse, elementType, resultType, projectionLambda);


        }

        /// <summary>
        /// The type of the result of Average aggregation method is: double
        /// </summary>
        /// <param name="elementType">>The type of entities</param>
        /// <param name="transformation">The transformation clause created by the parser</param>
        /// <returns>result type</returns>
        public override Type GetResultType(Type elementType, ApplyAggregateClause transformation)
        {
            return typeof(double);
        }
    }
}
