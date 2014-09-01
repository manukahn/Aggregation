using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.OData.OData.Query.Aggregation.QueriableImplementation
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

        public int MaxCollectionSize { get; set; }

        internal MethodCallConverter(IQueryProvider provider, Dictionary<Expression, QueryableRecord> baseCollections, int maxCollectionSize = 2000)
        {
            _provider = provider;
            MaxCollectionSize = maxCollectionSize;
            _baseCollections = baseCollections;
            GetRealQueriable_mi = typeof(MethodCallConverter)
                .GetMethods()
                .FirstOrDefault(mi => mi.IsGenericMethod && mi.Name == "GetRealQueriable");
        }

        public Expression Convert(Expression exp, int skip)
        {
            this._skip = skip;
            return this.Visit(exp);
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            IEnumerable<Expression> args = this.VisitExpressionList(m.Arguments);

            if ((args.Count() > 0) && (typeof(IQueryable).IsAssignableFrom(args.First().Type)))
            {
                var key = m.Arguments[0];
                var elementType = m.Method.GetGenericArguments().First();
                var newRecord = new QueryableRecord();

                if (_baseCollections.ContainsKey(key))
                {
                    var newArgs = new List<Expression>();
                    if (this._baseCollections[key].RealQueryable != null)
                    {
                        newArgs.Add(Expression.Constant(this._baseCollections[key].RealQueryable));
                    }
                    else
                    {
                        newArgs.Add(this._baseCollections[key].ConvertedExpression);
                    }

                    newArgs.AddRange(args.Skip(1));
                    var convertedExpression = Expression.Call(m.Object, m.Method, newArgs);
                    newRecord.ConvertedExpression = convertedExpression;
                    this._baseCollections.Add(m, newRecord);
                    return convertedExpression;
                }

                int index = _skip;
                bool limitReached = false;
                newRecord.LazyQueryable = this.ExtractValueFromExpression(m) as IQueryable;
                newRecord.RealQueryable = this.GetRealQueriable(m, elementType, ref index, out limitReached);
                newRecord.IndexInOriginalQueryable += index;
                newRecord.LimitReached = limitReached;
                this._baseCollections.Add(m, newRecord);

                return m;
            }
            return base.VisitMethodCall(m);
        }

        public IQueryable GetRealQueriable(MethodCallExpression m, Type elementType, ref int index, out bool limitReached)
        {
            limitReached = false;
            var mi = GetRealQueriable_mi.MakeGenericMethod(elementType);
            var getRealQueriableArgs = new object[] { m, index, limitReached };
            var res = mi.Invoke(this, getRealQueriableArgs) as IQueryable;
            index = (int)getRealQueriableArgs[1];
            limitReached = (bool)getRealQueriableArgs[2];
            return res;
        }


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


        private object ExtractValueFromExpression(Expression e)
        {
            if (e.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)e).Value;
            }

            LambdaExpression lambda = Expression.Lambda(e);
            Delegate fn = lambda.Compile();
            return fn.DynamicInvoke(null);
        }
    }
}
