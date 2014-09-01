using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.OData.OData.Query.Aggregation.QueriableImplementation
{
    /// <summary>
    /// Holds information about a queryable which might be split due to max page size.
    /// </summary>
    public class QueryableRecord
    {
        /// <summary>
        /// The queryable as a lazy statement
        /// </summary>
        public IQueryable LazyQueryable { get; set; }

        /// <summary>
        /// The queryable after enumeration
        /// </summary>
        public IQueryable RealQueryable { get; set; }

        /// <summary>
        /// The queryable expression after conversion into memory implementation
        /// </summary>
        public Expression ConvertedExpression { get; set; }

        /// <summary>
        /// If the queryable was split mark the index in the original queryable.
        /// </summary>
        public int IndexInOriginalQueryable { get; set; }

        /// <summary>
        /// Gets or sets indication if the queryable was split
        /// </summary>
        public bool? LimitReached { get; set; }
    }
}
