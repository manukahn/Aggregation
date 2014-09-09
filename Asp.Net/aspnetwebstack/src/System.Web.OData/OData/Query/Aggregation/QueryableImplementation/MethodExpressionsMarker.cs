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
    /// An expression visitor that is used to discover all the <see cref="MethodCallExpression"/> in an Expression Tree
    /// </summary>
    internal class MethodExpressionsMarker : ExpressionVisitorBase
    {
        public List<string> MethodsDiscovered = new List<string>();

        /// <summary>
        /// Discover all the <see cref="MethodCallExpression"/> in an Expression Tree
        /// </summary>
        /// <param name="exp">The expression tree to explore</param>
        /// <returns>The list of <see cref="MethodCallExpression"/> that were found in an Expression Tree</returns>
        public List<string> Eval(Expression exp)
        {
            this.Visit(exp);
            return MethodsDiscovered;
        }

        /// <summary>
        /// Override the basic strategy for visiting <see cref="MethodCallExpression"/> expressions.
        /// Discover all the <see cref="MethodCallExpression"/> in an Expression Tree and add them to a list
        /// </summary>
        /// <param name="m">The <see cref="MethodCallExpression"/> expression to visit</param>
        /// <returns>the expression after being visited</returns>
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            IEnumerable<Expression> args = this.VisitExpressionList(m.Arguments);
            if (!MethodsDiscovered.Contains(m.Method.Name))
            {
                MethodsDiscovered.Add(m.Method.Name);
            }

            return base.VisitMethodCall(m);
        }
    }
}
