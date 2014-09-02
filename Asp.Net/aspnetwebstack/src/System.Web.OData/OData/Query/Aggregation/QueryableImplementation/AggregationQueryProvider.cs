using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.OData.OData.Query.Aggregation.QueryableImplementation
{
    /// <summary>
    /// A query provider that wrap an <see cref="IQueryable"/> 
    /// and transform its queries to be executed on a memory collection if they are not supported by the query provider  
    /// </summary>
    public class AggregationQueryProvider : IQueryProvider
    {
        
        /// <summary>
        /// Create a new AggregationQueryProvider
        /// </summary>
        /// <param name="nativeQueryable">The <see cref="IQueryable"/> to wrap</param>
        /// <param name="maxResults">max number of results in a single transaction against a persistence provider</param>
        public AggregationQueryProvider(IQueryable nativeQueryable, int maxResults = 2000)
        {
           NativeQueryable = nativeQueryable;
           MaxResults = maxResults;
        }

        /// <summary>
        /// Gets or Sets a function that combines temporary results into one final result. 
        /// This function is used when the collection to query has more elements than allowed to query in a single transaction
        /// </summary>
        public Func<List<object>, object> Combiner { get; set; }

        /// <summary>
        /// Gets the <see cref="IQueryable"/> to wrap
        /// </summary>
        public IQueryable NativeQueryable { get; private set; }

        /// <summary>
        /// Gets the max number of elements that is allowed to query in a single transaction
        /// </summary>
        public int MaxResults { get; private set; }

        /// <summary>
        /// Create a query by using the wrapped <see cref="IQueryable"/>
        /// </summary>
        /// <typeparam name="TElement">Type of the element of the query</typeparam>
        /// <param name="expression">The query expression</param>
        /// <returns>A new query as an <see cref="IQueryable"/></returns>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            if (this.NativeQueryable == null)
            {
                throw new InvalidOperationException("null Native Queryable");
            }
            return this.NativeQueryable.Provider.CreateQuery<TElement>(expression);
        }

        /// <summary>
        /// Create a query by using the wrapped <see cref="IQueryable"/>
        /// </summary>
        /// <param name="expression">The query expression</param>
        /// <returns>A new query as an <see cref="IQueryable"/></returns>
        public IQueryable CreateQuery(Expression expression)
        {
            if (this.NativeQueryable == null)
            {
                throw new InvalidOperationException("null Native Queryable");
            }
            return this.NativeQueryable.Provider.CreateQuery(expression);
        }

        /// <summary>
        /// Executes the strongly-typed query represented by a specified expression tree. 
        /// If it is supported by the query provider execute it as is, otherwise convert it to be executed in memory.
        /// </summary>
        /// <typeparam name="TResult">Type of the element of the query</typeparam>
        /// <param name="expression">The expression tree to execute</param>
        /// <returns>The value that results from executing the specified query.</returns>
        public TResult Execute<TResult>(Expression expression)
        {
            if (this.NativeQueryable == null)
            {
                throw new InvalidOperationException("null Native Queryable");
            }

            try
            {
                return this.NativeQueryable.Provider.Execute<TResult>(expression);
            }
            catch (NotSupportedException ex)
            {
                var adapter = new QueriableProviderAdapter() { Provider = this.NativeQueryable.Provider, MaxCollectionSize = MaxResults };
                var res = adapter.Eval<TResult>(expression, Combiner);
                return res;
            }
        }

        /// <summary>
        /// Executes a query represented by a specified expression tree.
        /// If it is supported by the underlaying provider execute it as is, otherwise convert it to be executed in memory.
        /// </summary>
        /// <param name="expression">The expression tree to execute.</param>
        /// <returns>The value that results from executing the specified query.</returns>
        public object Execute(Expression expression)
        {
            if (this.NativeQueryable == null)
            {
                throw new InvalidOperationException("null Native Queryable");
            }

            try
            {
                return this.NativeQueryable.Provider.Execute(expression);
            }
            catch (NotSupportedException ex)
            {
                var adapter = new QueriableProviderAdapter() { Provider = this.NativeQueryable.Provider, MaxCollectionSize = MaxResults };
                var res = adapter.Eval(expression, Combiner);
                return res;
            }
        }
    }
}
