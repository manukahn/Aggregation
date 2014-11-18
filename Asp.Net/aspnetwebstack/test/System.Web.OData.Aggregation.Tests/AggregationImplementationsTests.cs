using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData.Aggregation.Tests.Common;
using System.Web.OData.OData.Query.Aggregation;
using System.Web.OData.OData.Query.Aggregation.SamplingMethods;
using FluentAssertions;
using Microsoft.OData.Core.UriParser.Semantic;
using Xbehave;
using Xunit;
using Xunit.Extensions;

namespace System.Web.OData.Aggregation.Tests
{
    public class AggregationImplementationsTests
    {
        [Scenario]
        public void RegisterImplementation()
        {
            "dayofweek is registered".Given(
                () =>
                {
                    AggregationImplementations<SamplingImplementationBase>.RegisterAggregationImplementation(
                        "dayofweek", new DayOfWeekSampling());
                });

            "when looking for the implementation".When(
                () =>
                    AggregationImplementations<SamplingImplementationBase>.GetAggregationImplementation("dayofweek")
                        .Should()
                        .NotBeNull());

        }

        [Scenario]
        public void LookForUnregisteredImplementation()
        {
            Exception exception = null;
            "dayofweek is registered".Given(
                () =>
                {
                    AggregationImplementations<SamplingImplementationBase>.RegisterAggregationImplementation(
                        "dayofweek", new DayOfWeekSampling());
                });

            "when looking for the implementation".When(
                () => exception =
                        Record.Exception(()=>AggregationImplementations<SamplingImplementationBase>.GetAggregationImplementation("xxx")));

            "unsupported Exception is thrown".Then(() => exception.Should().BeOfType<NotSupportedException>());

        }


        [Scenario]
        public void LookForDeletedImplementation()
        {
            Exception exception = null;
            "dayofweek is registered".Given(
                () =>
                {
                    AggregationImplementations<SamplingImplementationBase>.RegisterAggregationImplementation(
                        "dayofweek", new DayOfWeekSampling());
                });

            "dayofweek is removed".And(
                () =>
                {
                    AggregationImplementations<SamplingImplementationBase>.UnregisterAggregationImplementation("dayofweek");
                });

            "when looking for the implementation".When(
                () => exception =
                        Record.Exception(() => AggregationImplementations<SamplingImplementationBase>.GetAggregationImplementation("dayofweek")));

            "unsupported Exception is thrown".Then(() => exception.Should().BeOfType<NotSupportedException>());

        }

        [Scenario]
        [PropertyData("GetMethodsStrings")]
        public void ParseParamenters(string methodString)
        {
           string[] res = null;
           
            "Do parsing".When(()=> res = AggregationImplementationBase.GetAggregationParams(methodString));
            "check results".Then(() => res.Count().Should().Be(3));
            "check results".Then(() => res.First().Should().Be("a"));
            "check results".Then(() => res.Last().Should().Be("c"));
        }


        public static TheoryDataSet<string> GetMethodsStrings
        {
            get
            {
                return new TheoryDataSet<string>()
                {
                    "Sum(a,b,c)",
                    "Sum(((a,b,c)", 
                    "Sum(a,b,c)))", 
                };
            }
        }
    }
}
