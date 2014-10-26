using System.Linq;
using System.Web.OData.Query;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;

namespace System.Web.OData.OData.Query.Aggregation
{
    /// <summary>
    /// Implementation of Filter Aggregation transformation
    /// </summary>
    public class FilterImplementation : ApplyImplementationBase
    {
        /// <summary>
        ///  Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; set; }


        /// <summary>
        /// Do the filter operation
        /// </summary>
        /// <param name="query">The IQueryable to filter.</param>
        /// <param name="transformation">The filter query which was parsed.</param>
        /// <param name="querySettings">ODataQuerySettings.</param>
        /// <param name="queryOptionParser">The parser.</param>
        /// <returns>The filter result.</returns>
        public IQueryable DoFilter(IQueryable query, ApplyFilterClause transformation, ODataQuerySettings querySettings, ODataQueryOptionParser queryOptionParser)
        {
            string rawValue = transformation.RawQueryString;
            var queryOption = new FilterQueryOption(rawValue, this.Context, queryOptionParser);
            queryOption.FilterClause = transformation.Filter;
            return queryOption.ApplyTo(query, querySettings);
        }
    }
}
