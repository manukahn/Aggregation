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
    public abstract class ApplyQueryOptionTestBase
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


        public static TheoryDataSet<string, double> AggregationQueries
        {
            get
            {
                return new TheoryDataSet<string, double>()
                {
                    { "aggregate(Amount with sum as Result)", 440.0 },
                    { "aggregate(Product/TaxRate with sum as Result)", 142.8 },
                    { "aggregate(Amount mul 2 with sum as Result)",880.0 },
                    { "aggregate(Amount with min as Result)",20.0 },
                    { "aggregate(Amount with max as Result)", 100.0 },
                    { "aggregate(Amount with average as Result)",62.86 },
                    { "aggregate(Amount with countdistinct as Result)",5 },
                    { "aggregate(Amount with sumpower(2.0) as Result)", 35400},
                    { "groupby((Amount,Id), aggregate(Amount with sum as Result))",100.0 },
                    { "groupby((Product/TaxRate,Product/Category/Name,Id), aggregate(Amount with sum as Result))",100 },
                    { "groupby((Product/Category/Name), aggregate(Amount with sum as Result))",440.0 },
                    { "groupby((Product/TaxRate mul 2) aggregate(Amount mul 2 with sum as Result))",880.0},
                    { "groupby((Product/TaxRate mul 2 with round as RoundTax) aggregate(Amount with sum as Result))",440.0},
                    { "groupby((Time with dayofweek as day) aggregate(Amount with sum as Result))",100.0},
                    { "groupby(Amount mul 2) aggregate(Amount with sum as Result))",300.0},
                    { "groupby(Amount mul 2 with round as RoundAmount) aggregate(Amount with sum as Result))",300.0}
                };
            }
        }


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
                    { "groupby(Time with dayofweek as day)"},
                    { "groupby(Amount mul 2)"},
                    { "groupby(Amount mul 2 with round as RoundAmount)"},
                };
            }
        }


        protected ApplyQueryOption GetApplyQueryOption(string query)
        {
            string queryOption = "$apply";
            ODataQuerySettings settings = new ODataQuerySettings()
            {
                PageSize = 2000,
                HandleNullPropagation = HandleNullPropagationOption.False
            };

            var model = TestModelBuilder.CreateModel(new Type[] { typeof(Category), typeof(Product), typeof(Sales) });

            var context = new ODataQueryContext(model, typeof(Sales),
                new ODataPath(new ODataPathSegment[] { new EntitySetPathSegment("Sales") }));

            IEdmNavigationSource source = model.FindDeclaredEntitySet("Sales");

            var queryOptionParser = new ODataQueryOptionParser(model,
                model.FindDeclaredType("System.Web.OData.Aggregation.Tests.Common.Sales"),
                source,
                new Dictionary<string, string>() { { queryOption, query } });

            return new ApplyQueryOption(context, queryOptionParser);
        }

        protected IQueryable RunQuery(string query, int maxResults = 0, IQueryable data = null)
        {
            IQueryable results = null;
            ApplyQueryOption subject = null;

            ODataQuerySettings settings;
            EdmModel model;
            ODataQueryContext context;
            IAssembliesResolver assembliesResolver;
            GetApplyToParams(out settings, out model, out context, out assembliesResolver);
            if (maxResults != 0)
            {
                settings.PageSize = maxResults;
            }

            subject = GetApplyQueryOption(query);
            if (data == null)
            {
                data = TestDataSource.CreateData();
            }
            results = subject.ApplyTo(data, settings, assembliesResolver);
            return results;
        }

        protected static void GetApplyToParams(out ODataQuerySettings settings, out EdmModel model, out ODataQueryContext context, out IAssembliesResolver assembliesResolver)
        {
            settings = new ODataQuerySettings()
            {
                PageSize = 2000,
                HandleNullPropagation = HandleNullPropagationOption.False
            };

            assembliesResolver = new DefaultAssembliesResolver();
            model = TestModelBuilder.CreateModel(new Type[] { typeof(Category), typeof(Product), typeof(Sales) });
            context = new ODataQueryContext(model, typeof(Sales),
                new ODataPath(new ODataPathSegment[] { new EntitySetPathSegment("Sales") }));
        }
    

    }
}
