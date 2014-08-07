using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.OData.Extensions
{
    public static class CollectionExtesions
    {
        /// <summary>
        /// Get the second element in a collection if such exist 
        /// </summary>
        /// <typeparam name="T">element type of the collection</typeparam>
        /// <param name="collection">The collection to query</param>
        /// <returns>The second element</returns>
        public static T Second<T>(this IEnumerable<T> collection)
        {
            return collection.ToArray()[1];
        }
    }
}
