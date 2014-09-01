using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.OData.OData.Query.Aggregation.QueriableImplementation
{
    /// <summary>
    /// adapter that converts expressions not supported on a particular IQueruable provider to an in memory implementation
    /// </summary>
    internal class QueriableProviderAdapter
    {
        public IQueryProvider Provider { get; set; }

        public int MaxCollectionSize { get; set; }

        public TResult Eval<TResult>(Expression expression, Func<List<object>, object> combiner = null)
        {
            var baseCollections = new Dictionary<Expression, QueryableRecord>();
            var tempResults = new List<object>();
            TResult res = EvalImplementation<TResult>(expression, baseCollections, 0);
            

            var realRecord = baseCollections.Values.Where(record => record.LimitReached.HasValue && record.LimitReached.Value == true).FirstOrDefault();
            if (realRecord == null)
            {
                return res;
            }
           
            tempResults.Add(res);
            while (realRecord != null)
            {
                baseCollections.Clear();
                tempResults.Add(EvalImplementation<TResult>(expression, baseCollections, realRecord.IndexInOriginalQueryable));
                realRecord = baseCollections.Values.Where(record => record.LimitReached.HasValue && record.LimitReached.Value == true).FirstOrDefault();
            }

            if (combiner == null)
            {
                combiner = CombineTemporaryResults;
            }

            return (TResult)combiner(tempResults);
        }


        public object Eval(Expression expression, Func<List<object>, object> combiner = null)
        {
            var baseCollections = new Dictionary<Expression, QueryableRecord>();
            var tempResults = new List<object>();
            object res = EvalImplementation<object>(expression, baseCollections, 0);

            var realRecord = baseCollections.Values.Where(record => record.LimitReached.HasValue && record.LimitReached.Value == true).FirstOrDefault();
            if (realRecord == null)
            {
                return res;
            }

            tempResults.Add(res);
            while (realRecord != null)
            {
                baseCollections.Clear();
                tempResults.Add(EvalImplementation<object>(expression, baseCollections, realRecord.IndexInOriginalQueryable));
                realRecord = baseCollections.Values.Where(record => record.LimitReached.HasValue && record.LimitReached.Value == true).FirstOrDefault();
            }

            if (combiner == null)
            {
                combiner = CombineTemporaryResults;
            }

            return combiner(tempResults);
        }


        private IQueryable CombineTemporaryResults(List<object> temporaryResults)
        {
            if (!temporaryResults.Any())
            {
                return null;
            }

            Type elementType;
            if (temporaryResults.First() is IEnumerable<object>)
            {
                elementType = (temporaryResults.First() as IEnumerable<object>).First().GetType();
            }
            else
            {
                elementType = temporaryResults.First().GetType();
            }

            var finalRes = new List<object>();   
            foreach (var item in temporaryResults)
            {
                if (item is IEnumerable<object>)
                {
                    finalRes.AddRange(item as IEnumerable<object>);
                }
                else
                {
                    finalRes.Add(item);
                }
            }

            return ExpressionHelpers.Cast(elementType, finalRes.AsQueryable());
        }



        private TResult EvalImplementation<TResult>(Expression expression, Dictionary<Expression, QueryableRecord> baseCollections, int skip)
        {
            var converter = new MethodCallConverter(Provider, baseCollections, MaxCollectionSize);
            var newExp = converter.Convert(expression, skip);

            LambdaExpression lambda = Expression.Lambda(newExp);
            Delegate fn = lambda.Compile();
            return (TResult)fn.DynamicInvoke(null);
        }
        
        private static ConcurrentDictionary<string, List<string>> UnsupportedMethodsPerProvider =
            new ConcurrentDictionary<string, List<string>>();


        /// <summary>
        /// if the IQueriable expression is not supported by this provider convert the expression into memory implementation
        /// </summary>
        /// <param name="res"></param>
        /// <param name="maxResults"></param>
        /// <param name="convertedResult"></param>
        /// <returns></returns>
        public static bool ConvertionIsRequiredAsExpressionIfNotSupported(IQueryable res, int maxResults, out object convertedResult)
        {
            var vistor = new MethodExpressionsMarker();
            var methodsNames = vistor.Eval(res.Expression);
            var providerName = res.Provider.GetType().Name;
            List<string> knownUnsupportedFunctions;
            if (UnsupportedMethodsPerProvider.TryGetValue(providerName, out knownUnsupportedFunctions))
            {
                if (knownUnsupportedFunctions.Intersect(methodsNames).Count() > 0)
                {
                    var adapter = new QueriableProviderAdapter() { Provider = res.Provider, MaxCollectionSize = maxResults };
                    convertedResult = adapter.Eval(res.Expression);
                    return true; 
                }
            }
            
            try
            {
                var enumerator = res.GetEnumerator();
                enumerator.MoveNext();
                convertedResult = null;
                return false;
            }
            catch (NotSupportedException ex)
            {
                var unsupportedMethod = ex.Message.Split(' ').Intersect(methodsNames).First();
                UnsupportedMethodsPerProvider.AddOrUpdate(providerName,
                    (_) => new List<string>() { unsupportedMethod },
                    (_, lst) =>
                    {
                        lst.Add(unsupportedMethod);
                        return lst;
                    });


                var adapter = new QueriableProviderAdapter() { Provider = res.Provider, MaxCollectionSize = maxResults };
                convertedResult = adapter.Eval(res.Expression);
                return true;
            }
        }
    }
}
