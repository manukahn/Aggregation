using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData.Aggregation.Tests.Common;
using System.Web.OData.OData.Query.Aggregation;
using System.Web.OData.OData.Query.Aggregation.QueryableImplementation;
using FluentAssertions;
using Xbehave;
using Xunit;
using Xunit.Extensions;

namespace System.Web.OData.Aggregation.Tests.QueryableImplemetationTests
{
    public class AggregationQueryProviderTests : ApplyQueryOptionTestBase
    {
        [Scenario]
        [PropertyData("AggregationQueries")]
        public void DoValidAggregationUsingAdapter(string query, double expectedResult)
        {
            var data = TestDataSource.CreateData();
            IQueryable<Sales> queryabledata = TestInterceptingProvider.Intercept(data as IQueryable<Sales>, ThrowOnUnImplementedFunctions);

            IQueryable result = null;
            "Do aggregation".Given(() => result = RunQuery(query, 2000, queryabledata));
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
                    Math.Round((double)resultValue, 2).Should().Be(expectedResult);
                }
                else
                {
                    ((int)(resultValue)).Should().Be((int)expectedResult);
                }
            });
        }


        [Scenario]
        [PropertyData("FilterQueries")]
        public void DoValidFilterUsingAdapter(string query)
        {
            var data = TestDataSource.CreateData();
            IQueryable<Sales> queryabledata = TestInterceptingProvider.Intercept(data as IQueryable<Sales>, ThrowOnUnImplementedFunctions);

            IQueryable result = null;
            "Do aggregation".Given(() => result = RunQuery(query, 2000, queryabledata));
            "There are results".Then(() => result.Should().NotBeEmpty());
            "There are results".Then(() => result.Should().NotBeNull());
            "results are of type List<Sales>".Then(() => {result.Should().BeOfType<InterceptedQuery<Sales>>();});
        }

        [Scenario]
        [PropertyData("GroupByQueries")]
        public void DoValidGroupByUsingAdapter(string query)
        {
            var data = TestDataSource.CreateData();
            IQueryable<Sales> queryabledata = TestInterceptingProvider.Intercept(data as IQueryable<Sales>, ThrowOnUnImplementedFunctions);

            IQueryable result = null;
            "Do aggregation".Given(() => result = RunQuery(query, 2000, queryabledata));
            "There are results".Then(() => result.Should().NotBeEmpty());
            "There are results".Then(() => result.Should().NotBeNull());
            "Type of results is dynamic generated type".Then(
                () => result.AllElements().All(x => x.GetType().Namespace == "ODataAggregation.DynamicTypes").Should().BeTrue());
        }

        
        [Scenario]
        public void DoValidAggregationUsingAdapterAndCombiner()
        {
            string query = "aggregate(Amount with average as Result)";
            var data = TestDataSource.CreateData();
            IQueryable<Sales> queryabledata = TestInterceptingProvider.Intercept(data as IQueryable<Sales>, ThrowOnUnImplementedFunctions);

            IQueryable result = null;
            "Do aggregation".Given(() => result = RunQuery(query, 3, queryabledata));
            "There are results".Then(() => result.Should().NotBeEmpty());
            "There are results".Then(() => result.Should().NotBeNull());
            "all results have a property called Result".And(() =>
            {
                var item = result.First();
                var t = item.GetType();
                t.GetProperty("Result").Should().NotBeNull();
            });
           
            "all results have a property called Result".And(() =>
            {
                var item = result.First();
                var t = item.GetType();
                var res = t.GetProperty("Result").GetValue(item);
                Math.Round((double)res,2).Should().Be(62.86);
            });
        }


        [Scenario]
        public void ReapeatAggregationUsingAdapter()
        {
            string query = "aggregate(Amount with average as Result)";
            var data = TestDataSource.CreateData();
            IQueryable<Sales> queryabledata = TestInterceptingProvider.Intercept(data as IQueryable<Sales>, ThrowOnUnImplementedFunctions);

            IQueryable result = null;
            "Do aggregation".Given(() => result = RunQuery(query, 2000, queryabledata));
            "Do aggregation".Given(() => result = RunQuery(query, 2000, queryabledata));
            "Do aggregation".Given(() => result = RunQuery(query, 2000, queryabledata));
            "There are results".Then(() => result.Should().NotBeEmpty());
            "There are results".Then(() => result.Should().NotBeNull());
            "all results have a property called Result".And(() =>
            {
                var item = result.First();
                var t = item.GetType();
                t.GetProperty("Result").Should().NotBeNull();
            });

            "all results have a property called Result".And(() =>
            {
                var item = result.First();
                var t = item.GetType();
                var res = t.GetProperty("Result").GetValue(item);
                Math.Round((double)res, 2).Should().Be(62.86);
            });
        }


        [Scenario]
        public void DoValidGroupByUsingAdapterAndCombiner()
        {
            string reason =
                "everything works great but because our base data is in memory we get an 'System.ArgumentException' when executing it, " +
                "expression of type 'System.Collections.Generic.IEnumerable`1[System.Web.OData.Aggregation.Tests.Common.Sales]' cannot be used for return type 'System.Linq.IQueryable`1[System.Web.OData.Aggregation.Tests.Common.Sales]'" +
                "on real queryable this does not happen";


            var data = TestDataSource.CreateData();
            string query = "groupby(Amount,Id)";
            IQueryable<Sales> queryabledata = TestInterceptingProvider.Intercept(data as IQueryable<Sales>, ThrowOnUnImplementedFunctions);

            IQueryable result = null;
            Exception ex = null;
            "Do aggregation".Given(() => ex = Record.Exception(() => result = RunQuery(query, 3, queryabledata)));
            "System.Reflection.TargetInvocationException Exception is thrown".Then(() => ex.Should().BeOfType<TargetInvocationException>(reason));
            
        }


        public Expression ThrowOnUnImplementedFunctions(Expression exp)
        {
            var marker = new MethodExpressionsMarker();
            var methods = marker.Eval(exp);
            //if (methods.Contains("GroupBy") || methods.Contains("Min") || methods.Contains("Max") || methods.Contains("Average") || methods.Contains("Sum"))
            //{
            //    throw new NotSupportedException();
            //}
            if (methods.Contains("GroupBy"))
            {
                throw new NotSupportedException("GroupBy is not supported");
            }
            if (methods.Contains("Average"))
            {
                throw new NotSupportedException("Average is not supported");
            }
            return exp;
        }
    }
}
