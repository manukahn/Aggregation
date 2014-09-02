using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.OData.OData.Query.Aggregation.QueryableImplementation
{
    /// <summary>
    /// Wrapper class around IQueryable queries that executes using an AggregationQueryProvider 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AggregationQuery<T> : IQueryable, IQueryable<T>
    {
        IQueryProvider provider;
        IQueryable nativeQueryable;

        /// <summary>
        /// Create a new AggregationQuery
        /// </summary>
        /// <param name="nativeQueryable">The IQueryable to wrap</param>
        /// <param name="maxResults">max number of results in a single transaction against a persistence provider</param>
        public AggregationQuery(IQueryable nativeQueryable, int maxResults = 2000)
        {
            if (nativeQueryable == null)
            {
                throw new ArgumentNullException("nativeQueryable");
            }

            this.provider = new AggregationQueryProvider(nativeQueryable, maxResults); 
            this.nativeQueryable = nativeQueryable;
        }

        /// <summary>
        /// Create a new AggregationQuery that will execute with the provider provided
        /// </summary>
        /// <param name="nativeQueryable">The IQueryable to wrap</param>
        /// <param name="provider">The provider that will execute the query</param>
        public AggregationQuery(IQueryable nativeQueryable, IQueryProvider provider)
        {
            if (nativeQueryable == null)
            {
                throw new ArgumentNullException("nativeQueryable");
            }
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            this.provider = provider;
            this.nativeQueryable = nativeQueryable;
        }

        /// <summary>
        /// Gets the expression of the query to execute
        /// </summary>
        Expression IQueryable.Expression
        {
            get { return this.nativeQueryable.Expression; }
        }

        /// <summary>
        /// Gets the type of the elements on which the query runs
        /// </summary>
        Type IQueryable.ElementType
        {
            get { return nativeQueryable.ElementType; }
        }

        /// <summary>
        /// Gets the provider that will execute the query
        /// </summary>
        IQueryProvider IQueryable.Provider
        {
            get { return this.provider; }
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator<T>()
        {
            return ((IEnumerable<T>)this.provider.Execute(this.nativeQueryable.Expression)).GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.provider.Execute(this.nativeQueryable.Expression)).GetEnumerator();
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)this.provider.Execute(this.nativeQueryable.Expression)).GetEnumerator();
        }

    }
}
