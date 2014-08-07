using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData.Query;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Newtonsoft.Json.Schema;

namespace System.Web.OData.OData.Query.Aggregation
{
    public class FilterImplementation : ApplyImplementationBase
    {
        /// <summary>
        ///  Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; set; }

        public IQueryable DoFilter(IQueryable query, ApplyFilterClause transformation, ODataQuerySettings querySettings, ODataQueryOptionParser queryOptionParser)
        {
            string rawValue = transformation.RawQueryString;
            var queryOption = new FilterQueryOption(rawValue, this.Context, queryOptionParser);
            queryOption.FilterClause = transformation.Filter;
            return queryOption.ApplyTo(query, querySettings);
        }
    }
}
