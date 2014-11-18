using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;
using System.Web.OData.Aggregation.Tests.Common;
using System.Web.OData.OData.Query.Aggregation;
using System.Web.OData.Query;
using System.Web.OData.Query.Expressions;
using System.Web.OData.Routing;
using FluentAssertions;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using Xbehave;
using Xunit.Extensions;
using ODataPath = System.Web.OData.Routing.ODataPath;
using ODataPathSegment = System.Web.OData.Routing.ODataPathSegment;

namespace System.Web.OData.Aggregation.Tests
{
    public class GroupbyImplementationTest
    {
        public static TheoryDataSet<string> GroupingQueries
        {
            get
            {
                return new TheoryDataSet<string>()
                {
                    { "groupby(Amount,Id)" },
                    { "groupby(Product/TaxRate,Product/Category/Name,Id)" },
                    { "groupby(Product/Category/Name)" },
                    { "groupby(Product/TaxRate mul 2)"},
                    { "groupby(Product/TaxRate mul 2 with round as RoundTax)"},
                    { "groupby(Time with DayOfWeek as day)"},
                    { "groupby(Amount mul 2)"},
                    { "groupby(Amount mul 2 with round as RoundAmount)"},
                };
            }
        }


        public static TheoryDataSet<string> AggregatedGroupingQueries
        {
            get
            {
                return new TheoryDataSet<string>()
                {
                    { "groupby((Amount,Id), aggregate(Amount with sum as Total))" },
                    { "groupby((Product/TaxRate,Product/Category/Name,Id), aggregate(Amount with sum as Total))" },
                    { "groupby((Product/Category/Name), aggregate(Amount with sum as Total))" },
                    { "groupby((Product/TaxRate mul 2) aggregate(Amount mul 2 with sum as Total))"},
                    { "groupby((Product/TaxRate mul 2 with round as RoundTax) aggregate(Amount with sum as Total))"},
                    { "groupby((Time with DayOfWeek as day) aggregate(Amount with sum as Total))"},
                    { "groupby(Amount mul 2) aggregate(Amount with sum as Total))"},
                    { "groupby(Amount mul 2 with round as RoundAmount) aggregate(Amount with sum as Total))"}
                };
            }
        }

        [Scenario]
        [PropertyData("GroupingQueries")]
        public void DoGroupByValid(string query)
        {
            IQueryable results = null;
            "Do group by".Given(() => results = DoGroupBy(query));
            "Check valid results".Then(() =>
            {
                results.Should().NotBeEmpty();
            });
        }
        
        [Scenario]
        public void DoAggregatedGroupByValidCheckMethodSum()
        {
            
            IQueryable keys = null;
            object[] aggragatedValues = null;
            "Do groupby on Amount and Id".Given(() => DoAggregatedGroupBy("groupby((Amount), aggregate(Amount with sum as Total))", out keys, out aggragatedValues));
            "".Then(() =>
            {
                var key = keys.First();
                var t = key.GetType();
                t.GetProperty("Amount").Should().NotBeNull();
                ((double) t.GetProperty("Amount").GetValue(key)).Should().Be(100);

                var value = aggragatedValues.First();
               ((double)value).Should().Be(300);
            });

        }
        
        [Scenario]
        public void DoGroupByValidCheckResultSimpleProperties()
        {
            IQueryable results = null;
            "Do groupby on Amount and Id".Given(() => results = DoGroupBy("groupby(Amount,Id)"));
            "Check that Amount and ID exist in the key we produced".Then(() =>
            {
                var t = results.First().GetType();
                t.GetProperty("Amount").Should().NotBeNull();
                t.GetProperty("Id").Should().NotBeNull();
                t.GetProperties().Count().ShouldBeEquivalentTo(3); // Id, Amount, IEqualityComparer<T> ComparerInstance
            });
        }

        [Scenario]
        public void DoGroupByValidCheckResultComplexProperties()
        {
            IQueryable results = null;
            "".Given(() => results = DoGroupBy("groupby(Product/TaxRate,Product/Category/Name,Id)"));
            "".Then(() =>
            {
                var item = results.First();
                var t = item.GetType();
                t.GetProperty("Product").Should().NotBeNull();
                t.GetProperty("Id").Should().NotBeNull();
                t.GetProperties().Count().ShouldBeEquivalentTo(3); // Id, Product, IEqualityComparer<T> ComparerInstance
                var product = t.GetProperty("Product").GetValue(item);
                var p = product.GetType();
                p.GetProperty("TaxRate").Should().NotBeNull();
                p.GetProperty("Category").Should().NotBeNull();
                var category = p.GetProperty("Category").GetValue(product);
                var c = category.GetType();
                c.GetProperty("Name").Should().NotBeNull();
            });
        }
        
        [Scenario]
        public void DoGroupByValidCheckSamplingDayOfWeekMethods()
        {
            var days = "Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday";
            IQueryable results = null;
            "Do groupby on day of week of sales".Given(() => results = DoGroupBy("groupby(Time with DayOfWeek as day)"));
            "Result should be a day".Then(() =>
            {
                var item = results.First();
                var t = item.GetType();
                t.GetProperty("day").Should().NotBeNull();
                var day = t.GetProperty("day").GetValue(item);
                days.Contains(day.ToString()).Should().BeTrue();
            });
        }

        [Scenario]
        public void DoGroupByValidCheckSamplingHourInDayMethods()
        {
            
            IQueryable results = null;
            "Do groupby on hour in day of sales".Given(() => results = DoGroupBy("groupby(Time with hourinday(ETC) as hour)"));
            "Result should be a hour".Then(() =>
            {
                var item = results.First();
                var t = item.GetType();
                t.GetProperty("hour").Should().NotBeNull();
                var hour = t.GetProperty("hour").GetValue(item);
                var res = (int)hour <= 24;
                res.Should().BeTrue();

            });
        }
        
        [Scenario]
        public void DoGroupByValidCheckSamplingRoundMethods1()
        {
            IQueryable results = null;
            "Do groupby on round amount".Given(() => results = DoGroupBy("groupby(Amount with round as RoundAmount)"));
            "result should be a round number".Then(() =>
            {
                var item = results.First();
                var t = item.GetType();
                t.GetProperty("RoundAmount").Should().NotBeNull();
                var value = t.GetProperty("RoundAmount").GetValue(item);
                (((int)value * 100) % 100).ShouldBeEquivalentTo(0);
               
            });
        }

        [Scenario]
        public void DoGroupByValidCheckSamplingRoundMethods2()
        {
            IQueryable results = null;
            "Do groupby on day of week of sales".Given(() => results = DoGroupBy("groupby(Amount mul 2 with round as RoundAmount)"));
            "result should be a round number".Then(() =>
            {
                var item = results.First();
                var t = item.GetType();
                t.GetProperty("RoundAmount").Should().NotBeNull();
                var value = t.GetProperty("RoundAmount").GetValue(item);
                (((int)value * 100) % 100).ShouldBeEquivalentTo(0);

            });
        }
        

        [Scenario]
        [PropertyData("AggregatedGroupingQueries")]
        public void DoAggregatedGroupByValid(string query)
        {
            IQueryable keys = null;
            object[] aggragatedValues = null;
            "Do group by".Given(() => DoAggregatedGroupBy(query, out keys, out aggragatedValues));
            "Check valid results - keys".Then(() => keys.Should().NotBeEmpty());
            "Check valid results - aggregation values".Then(() => aggragatedValues.Should().NotBeEmpty());
        }

       
        private static void GetGroupByParams(string value, out IQueryable data, out ODataQueryContext context, out ODataQuerySettings settings,
            out ApplyGroupbyClause groupByClause, out DefaultAssembliesResolver assembliesResolver,
            out GroupByImplementation groupByImplementation, out Type keyType, out IEnumerable<LambdaExpression> propertiesToGroupByExpressions)
        {
            string queryOption = "$apply";
            data = TestDataSource.CreateData();
            settings = new ODataQuerySettings()
            {
                PageSize = 2000,
                HandleNullPropagation = HandleNullPropagationOption.False
            };
            var _settings = settings;

            var model = TestModelBuilder.CreateModel(new Type[] {typeof (Category), typeof (Product), typeof (Sales)});
            context = new ODataQueryContext(model, typeof (Sales),
                new ODataPath(new ODataPathSegment[] {new EntitySetPathSegment("Sales")}));
            var _context = context;

            IEdmNavigationSource source = model.FindDeclaredEntitySet("Sales");
            var parser = new ODataQueryOptionParser(model,
                model.FindDeclaredType("System.Web.OData.Aggregation.Tests.Common.Sales"),
                source,
                new Dictionary<string, string>() {{queryOption, value}});

            var applyCaluse = parser.ParseApply();
            groupByClause = applyCaluse.Transformations.First().Item2 as ApplyGroupbyClause;
            assembliesResolver = new DefaultAssembliesResolver();
            var _assembliesResolver = assembliesResolver;
            groupByImplementation = new GroupByImplementation() {Context = context};
            keyType = groupByImplementation.GetGroupByKeyType(groupByClause);
            var entityParam = Expression.Parameter(context.ElementClrType, "$it");
            propertiesToGroupByExpressions = groupByClause.SelectedPropertiesExpressions.Select(
                exp =>
                    FilterBinder.Bind(exp, _context.ElementClrType, _context.Model, _assembliesResolver, _settings, entityParam));
        }



        private IQueryable DoGroupBy(string value)
        {
            IQueryable data;
            ODataQueryContext context;
            ODataQuerySettings settings;
            ApplyGroupbyClause groupByClause;
            DefaultAssembliesResolver assembliesResolver;
            GroupByImplementation groupByImplementation;
            Type keyType;
            IEnumerable<LambdaExpression> propertiesToGroupByExpressions;
            GetGroupByParams(value, out data, out context, out settings, out groupByClause, out assembliesResolver, out groupByImplementation, out keyType, out propertiesToGroupByExpressions);
            
            return groupByImplementation.DoGroupBy(data, 2000, groupByClause, keyType, propertiesToGroupByExpressions);
        }


        private void DoAggregatedGroupBy(string value, out IQueryable keys, out object[] aggragatedValues)
        {
            IQueryable data;
            ODataQueryContext context;
            ODataQuerySettings settings;
            ApplyGroupbyClause groupByClause;
            DefaultAssembliesResolver assembliesResolver;
            GroupByImplementation groupByImplementation;
            Type keyType;
            IEnumerable<LambdaExpression> propertiesToGroupByExpressions;
            GetGroupByParams(value, out data, out context, out settings, out groupByClause, out assembliesResolver, out groupByImplementation, out keyType, out propertiesToGroupByExpressions);
            var propertyToAggregateExpression = FilterBinder.Bind(groupByClause.Aggregate.AggregatablePropertyExpression, context.ElementClrType, context.Model, assembliesResolver, settings);
            
            groupByImplementation.DoAggregatedGroupBy(data, 2000, groupByClause, keyType, propertiesToGroupByExpressions, propertyToAggregateExpression, out keys, out aggragatedValues);
        }
    }

}
