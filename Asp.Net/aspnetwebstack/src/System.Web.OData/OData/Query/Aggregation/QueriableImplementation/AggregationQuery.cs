using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.OData.OData.Query.Aggregation.QueriableImplementation
{

    public class AggregationQuery<T> : IQueryable, IQueryable<T>
    {
        IQueryProvider provider;
        IQueryable nativeQueryable;

        public AggregationQuery(IQueryable nativeQueryable, int maxResults = 2000)
        {
            if (nativeQueryable == null)
            {
                throw new ArgumentNullException("nativeQueryable");
            }

            this.provider = new AggregationQueryProvider(nativeQueryable, maxResults); 
            this.nativeQueryable = nativeQueryable;
        }

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

        Expression IQueryable.Expression
        {
            get { return this.nativeQueryable.Expression; }
        }

        Type IQueryable.ElementType
        {
            get { return nativeQueryable.ElementType; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return this.provider; }
        }

        public IEnumerator<T> GetEnumerator<T>()
        {
            return ((IEnumerable<T>)this.provider.Execute(this.nativeQueryable.Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.provider.Execute(this.nativeQueryable.Expression)).GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)this.provider.Execute(this.nativeQueryable.Expression)).GetEnumerator();
        }

    }
}
