using System.Linq;
using System.Linq.Expressions;
using System.Web.OData.Aggregation.Tests.Common;
using System.Web.OData.OData.Query.Aggregation;
using System.Web.OData.OData.Query.Aggregation.QueryableImplementation;
using FluentAssertions;
using Xbehave;
using Xunit.Extensions;


namespace System.Web.OData.Aggregation.Tests
{
    public class ApplyQueryOptionTests : ApplyQueryOptionTestBase
    {
        [Scenario]
        [PropertyData("AggregationQueries")]
        public void DoValidAggregationUsingApplyQueryOption(string query, double expectedResult)
        {
            IQueryable result = null;
            "Do aggregation".Given(() => result = RunQuery(query));
            "There are results".Then(() => result.Should().NotBeEmpty());
            "There are results".Then(() => result.Should().NotBeNull());
            "all results have a property called Result".And(() =>
            {
                var item = result.First();
                var t = item.GetType();
                t.GetProperty("Result").Should().NotBeNull();
            });
            "all results are correct".And(() =>
            {
                var item = result.First();
                var t = item.GetType();
                var prop = t.GetProperty("Result");
                var resultValue = prop.GetValue(item);
                if (prop.PropertyType == typeof(double))
                {
                    Math.Round((double) resultValue, 2).Should().Be(expectedResult);
                }
                else
                {
                    ((int)(resultValue)).Should().Be((int)expectedResult);
                }
            });
        }


        [Scenario]
        [PropertyData("FilterQueries")]
        public void DoValidFilterUsingApplyQueryOption(string query)
        {
            IQueryable result = null;
            "Do aggregation".Given(() => result = RunQuery(query));
            "There are results".Then(() => result.Should().NotBeEmpty());
            "There are results".Then(() => result.Should().NotBeNull());
            "results are of type List<Sales>".Then(() => { result.Should().BeOfType<InterceptedQuery<Sales>>(); });
        }

        [Scenario]
        [PropertyData("GroupByQueries")]
        public void DoValidGroupByUsingApplyQueryOption(string query)
        {
            IQueryable result = null;
            "Do aggregation".Given(() => result = RunQuery(query));
            "There are results".Then(() => result.Should().NotBeEmpty());
            "There are results".Then(() => result.Should().NotBeNull());
            "Type of results is dynamic generated type".Then(
                () => result.AllElements().All(x => x.GetType().Namespace == "ODataAggregation.DynamicTypes").Should().BeTrue());
        }

        [Scenario]
        public void FilterAndAggregate()
        {
            IQueryable result = null;
            string query = "filter(Amount gt 50)/aggregate(Amount with sum as Result)";
            "Do aggregation".Given(() => result = RunQuery(query));
            "There are results".Then(() =>
            {
                result.Should().NotBeNull();
            });
            "The result is correct".Then(() =>
            {
                var item = result.First();
                var t = item.GetType();
                ((double)t.GetProperty("Result").GetValue(item)).Should().Be(300);
            });

        }

    }


   
}
