using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData.Aggregation.Tests.Common;
using System.Web.OData.OData.Query.Aggregation;
using System.Web.OData.OData.Query.Aggregation.SamplingMethods;
using FluentAssertions;
using Xbehave;

namespace System.Web.OData.Aggregation.Tests
{
    public class ApplyImplementationBaseTests
    {
        [Scenario]
        public void GetProjectionExpressionTest()
        {
            var entityParam = Expression.Parameter(typeof(Sales), "e");
            var projectionExpression = ApplyImplementationBase.GetProjectionExpression(@"Product/Category/Name", entityParam);
            "After creating a projection expression".Then(() => projectionExpression.ToString().Should().BeEquivalentTo("e.Product.Category.Name"));
        }

        [Scenario]
        public void GetProjectionExpressionsTest()
        {
            var entityParam = Expression.Parameter(typeof(Sales), "e");
            var projectionExpressions = ApplyImplementationBase.GetProjectionExpressions(@"Product/Category/Name", entityParam);
            "After creating a projection expression".Then(() => projectionExpressions.Count.Should().Be(3));

            "First Expression".Then(() => projectionExpressions[0].ToString().Should().BeEquivalentTo("e.Product"));
            "Second Expression".Then(() => projectionExpressions[1].ToString().Should().BeEquivalentTo("e.Product.Category"));
            "Last Expression".Then(() => projectionExpressions[2].ToString().Should().BeEquivalentTo("e.Product.Category.Name"));
        }
        
         [Scenario]
        public void GetProjectionLambdaTest()
        {
            var entityParam = Expression.Parameter(typeof(Sales), "e");
            var projectionExpression = ApplyImplementationBase.GetProjectionLambda(typeof(Sales), @"Product/Category/Name");

            "After creating a projection lambda expression".Then(() => (projectionExpression as LambdaExpression).Should().NotBeNull());
            Console.WriteLine(projectionExpression.ToString());
            "After creating a projection lambda expression".Then(() => projectionExpression.ToString().Should().BeEquivalentTo("e => e.Product.Category.Name"));
        }

        
         [Scenario]
         public void GetMethodCallLambdaTest()
        {
            var mi = typeof(Sales).GetMethod("DummyInt");
            var projectionExpression = ApplyImplementationBase.GetMethodCallLambda(typeof(Sales), mi);

            "After creating a projection lambda expression".Then(() => (projectionExpression as LambdaExpression).Should().NotBeNull());
            Console.WriteLine(projectionExpression.ToString());
            "After creating a projection lambda expression".Then(() => projectionExpression.ToString().Should().BeEquivalentTo("e => DummyInt(e)"));
        }

        [Scenario]
        public void GetPropertyExpressionTest()
        {
            var entityParam = Expression.Parameter(typeof(Sales), "e");
            var result = ApplyImplementationBase.GetPropertyExpression(typeof (Sales), @"Product/Category/Name", entityParam, null);

            "after creating expression".Then(() => result.Should().NotBeNull());
            "After creating expression".Then(() => result.ToString().Should().BeEquivalentTo("IIF((e.Product == null), null, new Product() {Category = IIF((e.Product.Category == null), null, new Category() {Name = e.Product.Category.Name})})"));
        }


        [Scenario]
        public void GetComputedPropertyExpressionTest()
        {
            var entityParam = Expression.Parameter(typeof(Sales), "e");
            var mi = typeof(Sales).GetMethod("DummyString");
            var result = ApplyImplementationBase.GetComputedPropertyExpression(typeof(Sales), @"Product/Category/Name", entityParam, mi, null);

            "after creating expression".Then(() => result.Should().NotBeNull());
            "After creating expression".Then(() => result.ToString().Should().BeEquivalentTo
                ("IIF((e.Product == null), null, new Product() {Category = IIF((e.Product.Category == null), null, new Category() {Name = DummyString(e.Product.Category.Name)})})"));
        }


        [Scenario]
        public void GetPropertyInfoTest()
        {
            var result = ApplyImplementationBase.GetPropertyInfo(typeof(Sales), @"Product/Category/Name");
            "After creating the property infos".Then(() => result.Count.Should().Be(3));

            "First Expression".Then(() => result[0].Name.Should().BeEquivalentTo("Product"));
            "Second Expression".Then(() => result[1].Name.Should().BeEquivalentTo("Category"));
            "Last Expression".Then(() => result[2].Name.Should().BeEquivalentTo("Name"));
        }

        
    }
}
