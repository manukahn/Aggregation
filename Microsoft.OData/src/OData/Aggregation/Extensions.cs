using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.OData.Core.Aggregation
{
    public static class Extensions
    {
        public static int Find<T>(this T[] array, T elemenet)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(elemenet))
                    return i;
            }
            return -1;
        }

        public static string TrimOne(this string str, params char[] charactersToTrim)
        {
            int start = 0;
            int end = str.Length - 1;
            var removedFromStart = new List<char>();
            var removedFromEnd = new List<char>();

            for (int i = 0; i < charactersToTrim.Length; i++)
            {
                foreach (var c in charactersToTrim)
                {
                    if ((str[start] == c) && (!removedFromStart.Contains(c)))
                    {
                        start++;
                        removedFromStart.Add(c);
                    }
                    if ((str[end] == c) && (!removedFromEnd.Contains(c)))
                    {
                        end--;
                        removedFromEnd.Add(c);
                    }
                } 
            }
            return str.Substring(start, end - start + 1);

        }


        public static string TrimMethodCallPrefix(this string str)
        {
            var p = str.IndexOf('(');
            if (p == -1)
            {
                return str;
            }
            else
            {
                return str.Substring(p).Trim().Trim('(');
            }
        }

        public static string TrimMethodCallSufix(this string str)
        {
            return str.Trim().TrimOne(')');
            
        }
    }
}
