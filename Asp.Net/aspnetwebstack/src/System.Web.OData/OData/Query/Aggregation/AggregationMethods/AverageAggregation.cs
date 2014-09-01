using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http;
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

        /// <summary>
        /// Combine temporary results. This is useful when queryable is split due to max page size. 
        /// </summary>
        /// <param name="temporaryResults">The results to combine</param>
        /// <returns>The final result</returns>
        public override object CombineTemporaryResults(List<object> temporaryResults)
        {
            if (temporaryResults.Count() == 1)
                return temporaryResults[0];
            var t = temporaryResults[0].GetType();
            switch (t.Name)
            {
                case "Int32": return temporaryResults.Average(o => (int)o);
                case "Int64": return temporaryResults.Average(o => (long)o);
                case "Int16": return temporaryResults.Average(o => (short)o);
                case "Decimal": return temporaryResults.Average(o => (decimal)o);
                case "Double": return temporaryResults.Average(o => (double)o);
                case "Float": return temporaryResults.Average(o => (float)o);
            }

            throw Error.InvalidOperation("unsupported type");
        }
    }
}
