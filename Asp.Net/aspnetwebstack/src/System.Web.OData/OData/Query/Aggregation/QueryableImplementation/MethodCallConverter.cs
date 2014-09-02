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
    /// Converts method calls that uses unsupported expressions to use memory implementations
    /// </summary>
    internal class MethodCallConverter : ExpressionVisitor
    {
        private IQueryProvider _provider;
        private Dictionary<Expression, QueryableRecord> _baseCollections;
        private int _skip;
        private MethodInfo GetRealQueriable_mi;

        /// <summary>
        /// Gets the max number of results allowed in a single transaction against a persistence provider
        /// </summary>
        public int MaxCollectionSize { get; private set; }

        /// <summary>
        /// Create a new <see cref="MethodCallConverter"/>
        /// </summary>
        /// <param name="provider">The query provider that will execute the query</param>
        /// <param name="baseCollections">A container for collections to query</param>
        /// <param name="maxCollectionSize">The max number of results allowed in a single transaction against a persistence provider</param>
        internal MethodCallConverter(IQueryProvider provider, Dictionary<Expression, QueryableRecord> baseCollections, int maxCollectionSize = 2000)
        {
            _provider = provider;
            MaxCollectionSize = maxCollectionSize;
            _baseCollections = baseCollections;
            GetRealQueriable_mi = typeof(MethodCallConverter)
                .GetMethods()
                .FirstOrDefault(mi => mi.IsGenericMethod && mi.Name == "GetRealQueriable");
        }

        /// <summary>
        /// Convert the expression to use in-memory query implementation
        /// </summary>
        /// <param name="exp">The <see cref="Expression"/> to convert</param>
        /// <param name="skip">The number of elements to skip in the original collection before executing the query</param>
        /// <returns>An expression that expresses the original query but has no dependencies on a physical repository</returns>
        public Expression Convert(Expression exp, int skip)
        {
            this._skip = skip;
            return this.Visit(exp);
        }

        /// <summary>
        /// Override the basic strategy for visiting <see cref="MethodCallExpression"/> expressions.
        /// Converts methods that runs on <see cref="IQueryable"/> to run against a memory collection instead on a physical repository
        /// </summary>
        /// <param name="m">The <see cref="MethodCallExpression"/> expression to visit</param>
        /// <returns>the expression after being visited</returns>
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            IEnumerable<Expression> args = this.VisitExpressionList(m.Arguments);

            if ((args.Count() > 0) && (typeof(IQueryable).IsAssignableFrom(args.First().Type)))
            {
                var key = m.Arguments[0]; // get the collection on which this method should run
                var elementType = m.Method.GetGenericArguments().First();
                var newRecord = new QueryableRecord();

                if (_baseCollections.ContainsKey(key))
                {
                    /// create a new argument list that start with the collection to query
                    var newArgs = new List<Expression>();
                    if (this._baseCollections[key].RealQueryable != null)
                    {
                        newArgs.Add(Expression.Constant(this._baseCollections[key].RealQueryable));
                    }
                    else
                    {
                        newArgs.Add(this._baseCollections[key].ConvertedExpression);
                    }

                    newArgs.AddRange(args.Skip(1)); // paste the original arguments (except the first) to the new list
                    var convertedExpression = Expression.Call(m.Object, m.Method, newArgs); /// create a new <see cref="MethodCallExpression"/>
                    newRecord.ConvertedExpression = convertedExpression;
                    this._baseCollections.Add(m, newRecord);
                    return convertedExpression;
                }

                /// getting here means that the expression expresses the basic collection to query, without any method calls on top
                /// Here we create a record that contain an enumeration of this collection referred as "RealQueryable". 
                /// other expression (i.e. method calls) which are dependent on this basic collection will be altered to query against the RealQueryable
                int index = _skip;
                bool limitReached = false;
                newRecord.RealQueryable = this.GetRealQueriable(m, elementType, ref index, out limitReached);
                newRecord.IndexInOriginalQueryable += index;
                newRecord.LimitReached = limitReached;
                this._baseCollections.Add(m, newRecord);

                return m;
            }
            return base.VisitMethodCall(m);
        }

        /// <summary>
        /// Call the generic method GetRealQueriable<T>
        /// </summary>
        /// <param name="m">expression to enumerate</param>
        /// <param name="elementType">The type parameter to use</param>
        /// <param name="index">the index in the original collection after enumeration</param>
        /// <param name="limitReached">was the max number of elements allowed to query in a single transaction reached</param>
        /// <returns>an enumeration of the collection expressed in the <see cref="MethodCallExpression"/></returns>
        private IQueryable GetRealQueriable(MethodCallExpression m, Type elementType, ref int index, out bool limitReached)
        {
            limitReached = false;
            var mi = GetRealQueriable_mi.MakeGenericMethod(elementType);
            var getRealQueriableArgs = new object[] { m, index, limitReached };
            var res = mi.Invoke(this, getRealQueriableArgs) as IQueryable;
            index = (int)getRealQueriableArgs[1];
            limitReached = (bool)getRealQueriableArgs[2];
            return res;
        }

        /// <summary>
        /// Enumerate a collection expressed in the <see cref="MethodCallExpression"/> and bring it to memory
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection</typeparam>
        /// <param name="m">expression to enumerate</param>
        /// <param name="index">the index in the original collection after enumeration</param>
        /// <param name="limitReached">was the max number of elements allowed to query in a single transaction reached</param>
        /// <returns>an enumeration of the collection expressed in the <see cref="MethodCallExpression"/></returns>
        public IQueryable GetRealQueriable<T>(MethodCallExpression m, ref int index, out bool limitReached)
        {
            var methodToCall = m;
            if (this._skip > 0)
            {
                methodToCall = ExpressionHelpers.Skip(m, this._skip, typeof(T));
            }
            var res = this._provider.Execute(methodToCall);
            var realCollection = new List<T>();
            if (res is IEnumerable<T>)
            {
                int counter = 0;
                limitReached = false;
                var enumerator = (res as IEnumerable<T>).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    counter++;
                    if (counter <= this.MaxCollectionSize)
                    {
                        realCollection.Add(enumerator.Current);
                        index++;
                    }
                    else
                    {
                        limitReached = true;
                        return realCollection.AsQueryable();
                    }
                }
                return realCollection.AsQueryable();
            }
            else
            {
                throw new InvalidOperationException("MethodCallExpression does not produce an IQueryable");
            }
        }
    }
}
