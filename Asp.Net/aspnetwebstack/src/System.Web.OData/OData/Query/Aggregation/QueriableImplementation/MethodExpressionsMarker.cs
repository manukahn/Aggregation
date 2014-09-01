using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.OData.OData.Query.Aggregation.QueriableImplementation
{
    public class MethodExpressionsMarker : ExpressionVisitor
    {
        public List<string> MethodsDiscovered = new List<string>();

        public List<string> Eval(Expression exp)
        {
            this.Visit(exp);
            return MethodsDiscovered;
        }

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
