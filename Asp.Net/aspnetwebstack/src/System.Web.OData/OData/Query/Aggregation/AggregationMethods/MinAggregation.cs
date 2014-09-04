﻿using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http;
using Microsoft.OData.Core.UriParser.Semantic;

namespace System.Web.OData.OData.Query.Aggregation.AggregationMethods
{
    /// <summary>
    /// Implementation of Min aggregation method
    /// </summary>
    public class MinAggregation : AggregationImplementationBase
    {
        /// <summary>
        /// Implement the Min aggregation method
        /// </summary>
        /// <param name="elementType">The type of entities</param>
        /// <param name="query">The collection</param>
        /// <param name="transformation">The transformation clause created by the parser</param>
        /// <param name="propertyToAggregateExpression">Projection Expression that defines access to the property to aggregate</param>
        /// <returns>The Min result</returns>
        public override object DoAggregatinon(Type elementType, IQueryable query, ApplyAggregateClause transformation, LambdaExpression propertyToAggregateExpression)
        {
            IQueryable queryToUse = query;
            if (transformation.AggregatableProperty.Contains('/'))
            {
                queryToUse = FilterNullValues(query, elementType, transformation);
            }

            var projectionLambda = GetProjectionLambda(elementType, transformation, propertyToAggregateExpression);
            var resultType = this.GetResultType(elementType, transformation);
            
            var selected = (ExpressionHelpers.QueryableSelect(queryToUse, elementType, resultType, projectionLambda)).AsQueryable();

            //call: (selected.AsQueryable() as IQueryable<double>).Min();
            return ExpressionHelpers.Min(resultType, selected);

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
                case "Int32": return temporaryResults.Min(o => (int)o);
                case "Int64": return temporaryResults.Min(o => (long)o);
                case "Int16": return temporaryResults.Min(o => (short)o);
                case "Decimal": return temporaryResults.Min(o => (decimal)o);
                case "Double": return temporaryResults.Min(o => (double)o);
                case "Float": return temporaryResults.Min(o => (float)o);
            }

            throw Error.InvalidOperation("unsupported type");
        }
    }
}
