using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData.OData.Query.Aggregation;
using System.Web.OData.OData.Query.Aggregation.SamplingMethods;
using FluentAssertions;
using Xbehave;
using Xunit;

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
    }
}
