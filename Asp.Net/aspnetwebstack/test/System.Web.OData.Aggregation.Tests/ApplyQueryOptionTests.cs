using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dispatcher;
using System.Web.OData.Aggregation.Tests.Common;
using System.Web.OData.OData.Query;
using System.Web.OData.OData.Query.Aggregation;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using FluentAssertions;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Xbehave;
using Xunit.Extensions;
using IEdmType = Microsoft.Data.Edm.IEdmType;
using ODataPath = System.Web.OData.Routing.ODataPath;
using ODataPathSegment = System.Web.OData.Routing.ODataPathSegment;

namespace System.Web.OData.Aggregation.Tests
{
    public class ApplyQueryOptionTests
    {

        public static TheoryDataSet<string> FilterQueries
        {
            get
            {
                return new TheoryDataSet<string>()
                {
                    { "filter(Amount gt 50)" },
                    { "filter(Amount gt 20)" },
                };
            }
        }


        public static TheoryDataSet<string> AggregationQueries
        {
            get
            {
                return new TheoryDataSet<string>()
                {
                    { "aggregate(Amount with sum as Result)" },
                    { "aggregate(Product/TaxRate with sum as Result)" },
                    { "aggregate(Amount mul 2 with sum as Result)" },
                    { "aggregate(Amount with min as Result)" },
                    { "aggregate(Amount with max as Result)" },
                    { "aggregate(Amount with average as Result)" },
                    { "aggregate(Amount with countdistinct as Result)" },
                    { "groupby((Amount,Id), aggregate(Amount with sum as Result))" },
                    { "groupby((Product/TaxRate,Product/Category/Name,Id), aggregate(Amount with sum as Result))" },
                    { "groupby((Product/Category/Name), aggregate(Amount with sum as Result))" },
                    { "groupby((Product/TaxRate mul 2) aggregate(Amount mul 2 with sum as Result))"},
                    { "groupby((Product/TaxRate mul 2 with round as RoundTax) aggregate(Amount with sum as Result))"},
                    { "groupby((Time with DayOfWeek as day) aggregate(Amount with sum as Result))"},
                    { "groupby(Amount mul 2) aggregate(Amount with sum as Result))"},
                    { "groupby(Amount mul 2 with round as RoundAmount) aggregate(Amount with sum as Result))"}
                };
            }
        }


        //public static TheoryDataSet<string> AggregatedGroupByQueries
        //{
        //    get
        //    {
        //        return new TheoryDataSet<string>()
        //        {
        //            { "groupby((Amount,Id), aggregate(Amount with sum as Total))" },
        //            { "groupby((Product/TaxRate,Product/Category/Name,Id), aggregate(Amount with sum as Total))" },
        //            { "groupby((Product/Category/Name), aggregate(Amount with sum as Total))" },
        //            { "groupby((Product/TaxRate mul 2) aggregate(Amount mul 2 with sum as Total))"},
        //            { "groupby((Product/TaxRate mul 2 with round as RoundTax) aggregate(Amount with sum as Total))"},
        //            { "groupby((Time with DayOfWeek as day) aggregate(Amount with sum as Total))"},
        //            { "groupby(Amount mul 2) aggregate(Amount with sum as Total))"},
        //            { "groupby(Amount mul 2 with round as RoundAmount) aggregate(Amount with sum as Total))"}
        //        };
        //    }
        //}


        public static TheoryDataSet<string> GroupByQueries
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

        public ApplyQueryOption GetApplyQueryOption(string query)
        {
            string queryOption = "$apply";
            ODataQuerySettings settings = new ODataQuerySettings()
            {
                PageSize = 2000,
                HandleNullPropagation = HandleNullPropagationOption.False
            };

            var model = TestModelBuilder.CreateModel(new Type[] {typeof(Category), typeof(Product), typeof(Sales)});

            var context = new ODataQueryContext(model, typeof (Sales),
                new ODataPath(new ODataPathSegment[] {new EntitySetPathSegment("Sales")}));

            IEdmNavigationSource source = model.FindDeclaredEntitySet("Sales");

            var queryOptionParser = new ODataQueryOptionParser(model,
                model.FindDeclaredType("System.Web.OData.Aggregation.Tests.Common.Sales"),
                source,
                new Dictionary<string, string>() {{queryOption, query}});

            return new ApplyQueryOption(context, queryOptionParser);
        }


        [Scenario]
        [PropertyData("AggregationQueries")]
        public void DoValidAggregationUsingApplyQueryOption(string query)
        {
            IQueryable result = null;
            "Do aggregation".Given(() => result = RunQuery(query));
            "There are results".Then(() => result.Should().NotBeEmpty());
            "all results have a property called Result".And(() =>
            {
                var item = result.First();
                var t = item.GetType();
                t.GetProperty("Result").Should().NotBeNull();
            });
        }


        [Scenario]
        [PropertyData("FilterQueries")]
        public void DoValidFilterUsingApplyQueryOption(string query)
        {
            IQueryable result = null;
            "Do aggregation".Given(() => result = RunQuery(query));
            "There are results".Then(() => result.Should().NotBeEmpty());
            "results are of type List<Sales>".Then(() => result.Should().BeOfType<EnumerableQuery<Sales>>());
        }


       
        private IQueryable RunQuery(string query)
        {
            IQueryable data = null;
            IQueryable results = null;
            ApplyQueryOption subject = null;

            ODataQuerySettings settings;
            EdmModel model;
            ODataQueryContext context;
            IAssembliesResolver assembliesResolver;
            GetApplyToParams(out settings, out model, out context, out assembliesResolver);

            subject = GetApplyQueryOption(query);
            data = TestDataSource.CreateData();
            results = subject.ApplyTo(data, settings, assembliesResolver);
            return results;
        }

        private static void GetApplyToParams(out ODataQuerySettings settings, out EdmModel model, out ODataQueryContext context, out IAssembliesResolver assembliesResolver)
        {
            settings = new ODataQuerySettings()
            {
                PageSize = 2000,
                HandleNullPropagation = HandleNullPropagationOption.False
            };

            assembliesResolver = new DefaultAssembliesResolver();
            model = TestModelBuilder.CreateModel(new Type[] {typeof(Category), typeof(Product), typeof(Sales)});
            context = new ODataQueryContext(model, typeof (Sales),
                new ODataPath(new ODataPathSegment[] {new EntitySetPathSegment("Sales")}));
        }
    }


   
}
