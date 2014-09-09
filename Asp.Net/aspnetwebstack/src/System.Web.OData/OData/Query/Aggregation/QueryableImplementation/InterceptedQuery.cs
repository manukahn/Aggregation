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
    /// An IQueryable query object that is executed by an InterceptingProvider 
    /// </summary>
    public class InterceptedQuery<T> : IQueryable, IQueryable<T>
    {
        private Expression _expression;
        private InterceptingProvider _provider;

        /// <summary>
        /// Create a new query object 
        /// </summary>
        /// <param name="provider">The InterceptingProvider that will execute the query</param>
        /// <param name="expression">The expression to run</param>
        public InterceptedQuery(
           InterceptingProvider provider,
           Expression expression)
        {
            this._provider = provider;
            this._expression = expression;
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return this._provider.ExecuteQuery<T>(this._expression);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._provider.ExecuteQuery<T>(this._expression);
        }
        /// <summary>
        /// Gets the type of the elements on which the query runs
        /// </summary>
        public Type ElementType
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// Gets the expression to execute
        /// </summary>
        public Expression Expression
        {
            get { return this._expression; }
        }

        /// <summary>
        /// Gets the provider of the query
        /// </summary>
        public IQueryProvider Provider
        {
            get { return this._provider; }
        }
    }
}
