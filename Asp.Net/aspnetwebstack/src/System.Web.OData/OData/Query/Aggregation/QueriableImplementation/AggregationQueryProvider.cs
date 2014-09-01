using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.OData.OData.Query.Aggregation.QueriableImplementation
{
    public class AggregationQueryProvider : IQueryProvider
    {
        /// <summary>
        /// function that combines temporary results into one final result.
        /// </summary>
        public Func<List<object>, object> Combiner { get; set; }

        public AggregationQueryProvider(IQueryable nativeQueryable, int maxResults = 2000)
       {
           NativeQueryable = nativeQueryable;
           MaxResults = maxResults;
       }

       public IQueryable NativeQueryable { get; set; }

       public int MaxResults { get; set; }
       

       public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
       {
           if (this.NativeQueryable == null)
           {
               throw new InvalidOperationException("null Native Queryable");
           }
           return this.NativeQueryable.Provider.CreateQuery<TElement>(expression);
       }

       public IQueryable CreateQuery(Expression expression)
       {
           if (this.NativeQueryable == null)
           {
               throw new InvalidOperationException("null Native Queryable");
           }
           return this.NativeQueryable.Provider.CreateQuery(expression);
       }

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
