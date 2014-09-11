using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData.Aggregation.Tests.Common;
using System.Web.OData.OData.Query.Aggregation;
using System.Web.OData.OData.Query.Aggregation.AggregationMethods;
using FluentAssertions;
using Microsoft.OData.Core.UriParser.Semantic;
using Xbehave;
using Xunit;
using Xunit.Extensions;

namespace System.Web.OData.Aggregation.Tests
{
    public class AggregationImplementationBaseTests : AggregationImplementationBase
    {
        [Scenario]
        public void GetAggregatedPropertyTypeWithValidPath()
        {
            Type typeToExplore = typeof(Sales);

            "When calling GetAggregatedPropertyType".When(
                () =>
                    this.GetAggregatedPropertyType(typeToExplore, "Product/Category/Name")
                        .ShouldBeEquivalentTo(typeof(string)));
        }

        [Scenario]
        public void GetAggregatedPropertyTypeWithInvalidPath()
        {
            Type typeToExplore = typeof(Sales);

            Exception exception = null;
            "When calling GetAggregatedPropertyType with invalid path".When(
                () =>
                {
                    exception =
                        Record.Exception(
                            () => this.GetAggregatedPropertyType(typeToExplore, "Product//////Category/////Name"));
                });

            "Then Invalid Operation Exception is thrown".Then(() => exception.Should().BeOfType<InvalidOperationException>());
        }

        [Scenario]
        public void GetAggregatedPropertyTypeWithNullPath()
        {
            Exception exception = null;
            "When calling GetAggregatedPropertyType with invalid path".When(
                () =>
                {
                    exception =
                        Record.Exception(
                            () => this.GetAggregatedPropertyType(null, null));
                });

            "Then Invalid Operation Exception is thrown".Then(() => exception.Should().BeOfType<NullReferenceException>());
        }


        [Scenario]
        [PropertyData("GetClause")]
        public void FilterNullValuesWithValidArgs(ApplyAggregateClause clause)
        {
            if (clause.AggregatableProperty.Contains('/'))
            {
                this.FilterNullValuesWithValidArgsComplexProperty(clause);
            }
            else
            {
                this.FilterNullValuesWithValidArgsSimpleProperty(clause);
            }
        }

        public void FilterNullValuesWithValidArgsSimpleProperty(ApplyAggregateClause clause)
        {
            var data = TestDataSource.CreateData();
            "path is a simple property".Given(() => clause.AggregatableProperty.Contains('/').Should().BeFalse());
            "When calling FilterNullValues".When(
                () =>
                {
                    var res = FilterNullValues(data, typeof (Sales), clause);
                    (res == data).Should().BeTrue();
                });
        }


        public void FilterNullValuesWithValidArgsComplexProperty(ApplyAggregateClause clause)
        {
            "path is a complex property".Given(() => clause.AggregatableProperty.Contains('/').Should().BeTrue());
            "When calling FilterNullValues".When(
                () =>
                    FilterNullValues(TestDataSource.CreateData(), typeof(Sales), clause)
                        .Expression.ToString()
                        .ShouldBeEquivalentTo(
                        "System.Collections.Generic.List`1[System.Web.OData.Aggregation.Tests.Common.Sales].Where(e => (e.Product != null)).Where(e => (e.Product.Category != null))"));
        }

        public static TheoryDataSet<ApplyAggregateClause> GetClause
        {
            get
            {
                return new TheoryDataSet<ApplyAggregateClause>()
                {
                    new ApplyAggregateClause(){ AggregatableProperty = "Amount", AggregationMethod = "Sum", Alias = "Total"},
                    new ApplyAggregateClause(){ AggregatableProperty = "Product/Category/Name", AggregationMethod = "Sum", Alias = "Total"}
                };
            }
        }


        public override object DoAggregatinon(Type elementType, IQueryable query, Microsoft.OData.Core.UriParser.Semantic.ApplyAggregateClause transformation, Linq.Expressions.LambdaExpression propertyToAggregateExpression)
        {
            throw new NotImplementedException();
        }

        public override Type GetResultType(Type elementType, Microsoft.OData.Core.UriParser.Semantic.ApplyAggregateClause transformation)
        {
            throw new NotImplementedException();
        }

       
        public override object CombineTemporaryResults(List<Tuple<object, int>> temporaryResults)
        {
            throw new NotImplementedException();
        }
    }
}
