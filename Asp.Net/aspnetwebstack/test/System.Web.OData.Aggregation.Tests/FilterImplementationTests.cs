using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.OData.Aggregation.Tests.Common;
using System.Web.OData.OData.Query.Aggregation;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using FluentAssertions;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using Xbehave;
using Xunit.Extensions;
using IEdmType = Microsoft.Data.Edm.IEdmType;
using ODataPath = System.Web.OData.Routing.ODataPath;
using ODataPathSegment = System.Web.OData.Routing.ODataPathSegment;


namespace System.Web.OData.Aggregation.Tests
{
    public class FilterImplementationTests
    {
        public static TheoryDataSet<string> Queries
        {
            get
            {
                return new TheoryDataSet<string>()
                {
                    { "filter(Amount gt 50)" },
                    { "filter(Amount gt 100)" },
                    { "filter(Amount gt 2)" },
                };
            }
        }

        [Scenario]
        [PropertyData("Queries")]
        public void DoFilterValidQuery(string query)
        {
            if (query.Contains("50"))
            {
                FilterGt50(query,50);
            }
            else if (query.Contains("100"))
            {
                FilterGt100(query, 100);
            }
            else if (query.Contains("2"))
            {
                FilterGt2(query, 2);
            }

        }


        public void FilterGt50(string query, int data)
        {
            IQueryable<Sales> result = null;
            "exection of apply filter".Given(() => { result = DoQuery(query) as IQueryable<Sales>; });
            "result is IQueryable<Sales>".Then(() => result.Should().NotBeNull());
            "only 3 records passed the filter".Then(() => result.Count().ShouldBeEquivalentTo(3));
            "filter was done".Then(() => result.All(item => item.Amount > data).Should().BeTrue());
        }

        public void FilterGt100(string query, int data)
        {
            IQueryable<Sales> result = null;
            "exection of apply filter".Given(() => { result = DoQuery(query) as IQueryable<Sales>; });
            "result is IQueryable<Sales>".Then(() => result.Should().NotBeNull());
            "only 3 records passed the filter".Then(() => result.Count().ShouldBeEquivalentTo(0));
            "filter was done".Then(() => result.All(item => item.Amount > data).Should().BeTrue());
        }

        public void FilterGt2(string query, int data)
        {
            IQueryable<Sales> result = null;
            "exection of apply filter".Given(() => { result = DoQuery(query) as IQueryable<Sales>; });
            "result is IQueryable<Sales>".Then(() => result.Should().NotBeNull());
            "only 3 records passed the filter".Then(() => result.Count().ShouldBeEquivalentTo(7));
            "filter was done".Then(() => result.All(item => item.Amount > data).Should().BeTrue());
        }

        
        public IQueryable DoQuery(string value)
        {
            string queryOption = "$apply";
            var data = TestDataSource.CreateData();
            ODataQuerySettings settings = new ODataQuerySettings() {PageSize = 2000, HandleNullPropagation = HandleNullPropagationOption.False};
            var model = TestModelBuilder.CreateModel(new Type[] { typeof(Category), typeof(Product), typeof(Sales) });
            var context = new ODataQueryContext(model, typeof(Sales), new ODataPath(new ODataPathSegment[] { new EntitySetPathSegment("Sales") }));

            IEdmNavigationSource source = model.FindDeclaredEntitySet("Sales");
             var parser = new ODataQueryOptionParser(model, 
                model.FindDeclaredType("System.Web.OData.Aggregation.Tests.Common.Sales"),
                source,
                new Dictionary<string, string>() { { queryOption, value } });

            var applyClause = parser.ParseApply();
            var filterClause = applyClause.Transformations.First().Item2 as ApplyFilterClause;

            var filter = new FilterImplementation() { Context = context };
            return filter.DoFilter(data, filterClause, settings, parser);
        }
    }
}
