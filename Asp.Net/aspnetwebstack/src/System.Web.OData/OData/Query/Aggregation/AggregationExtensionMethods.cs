using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.OData.OData.Query.Aggregation
{
    public static class AggregationExtensionMethods
    {
        /// <summary>
        /// Gets the first element in an IQueriable.
        /// </summary>
        /// <param name="queryable">IQueriable</param>
        /// <returns>The first element</returns>
        public static object First(this IQueryable queryable)
        {
            var enumerator = queryable.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current;
        }

        /// <summary>
        /// Convert an IQueryable to a list of objects.
        /// </summary>
        /// <param name="queryable">IQueriable</param>
        /// <returns>List of objects</returns>
        public static List<object> AllElements(this IQueryable queryable)
        {
            return AllElements<object>(queryable).AsParallel().ToList();
        }
        

        public static IList<T> AllElements<T>(this IQueryable queryable)
        {
            return (queryable as IEnumerable).Cast<T>().AsParallel().ToList();
        }

        /// <summary>
        /// Find an IEnumerable in a type.
        /// </summary>
        /// <param name="seqType">The type to explore</param>
        /// <returns>The type of the IEnumerable that was found</returns>
        public static Type FindIEnumerable(this Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
                return null;
            if (seqType.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            if (seqType.IsGenericType)
            {
                foreach (Type arg in seqType.GetGenericArguments())
                {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType))
                    {
                        return ienum;
                    }
                }
            }
            Type[] ifaces = seqType.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null) return ienum;
                }
            }
            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
            {
                return FindIEnumerable(seqType.BaseType);
            }
            return null;
        }
    }
}
