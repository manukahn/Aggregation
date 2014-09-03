using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http;
using Microsoft.OData.Core.UriParser.Semantic;

namespace System.Web.OData.OData.Query.Aggregation.AggregationMethods
{
    public class CountDistinctAggregation : AggregationImplementationBase
    {
        /// <summary>
        /// Implement the Count-Distinct aggregation method
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
            var propertyType = GetAggregatedPropertyType(elementType, transformation.AggregatableProperty);
            var selected = (ExpressionHelpers.Select(queryToUse, elementType, propertyType, projectionLambda)).AsQueryable();

            //call: (selected.AsQueryable() as IQueryable<double>).Ditinct();
            var distinct = ExpressionHelpers.Distinct(propertyType, selected);

            //call: (distinct.AsQueryable() as IQueryable<double>).Count();
            return ExpressionHelpers.Count(propertyType, distinct);
        }

        /// <summary>
        /// The type of the result of Count-Distinct aggregation method is: int
        /// </summary>
        /// <param name="elementType">>The type of entities</param>
        /// <param name="transformation">The transformation clause created by the parser</param>
        /// <returns>result type</returns>
        public override Type GetResultType(Type elementType, ApplyAggregateClause transformation)
        {
            return typeof(int);
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
                case "Int32": return temporaryResults.Sum(o => (int)o);
                case "Int64": return temporaryResults.Sum(o => (long)o);
                case "Int16": return temporaryResults.Sum(o => (short)o);
                case "Decimal": return temporaryResults.Sum(o => (decimal)o);
                case "Double": return temporaryResults.Sum(o => (double)o);
                case "Float": return temporaryResults.Sum(o => (float)o);
            }

            throw Error.InvalidOperation("unsupported type");
        }
    }
}
