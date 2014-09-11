using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData.OData.Query.Aggregation;
using System.Web.OData.OData.Query.Aggregation.AggregationMethods;
using System.Web.OData.OData.Query.Aggregation.SamplingMethods;
using FluentAssertions;
using Xbehave;
using Xunit;

namespace System.Web.OData.Aggregation.Tests
{
    [AggregationMethod("firstplusone")]
    public class MySimpleAggergationMethods : AggregationImplementationBase
    {
        public static double FirstPlusOne(IQueryable input)
        {
            return (double) input.First() + 1;
        }

        public override object DoAggregatinon(Type elementType, IQueryable query, Microsoft.OData.Core.UriParser.Semantic.ApplyAggregateClause transformation, Linq.Expressions.LambdaExpression propertyToAggregateExpression)
        {
           var resultType = this.GetResultType(elementType, transformation);
           var selected = GetItemsToQuery(elementType, query, propertyToAggregateExpression, resultType);

           MethodInfo minMethod = this.GetType().GetMethod("FirstPlusOne");
           return minMethod.Invoke(null, new[] { selected });
        }

        public override Type GetResultType(Type elementType, Microsoft.OData.Core.UriParser.Semantic.ApplyAggregateClause transformation)
        {
            return typeof(double);
        }

        public override object CombineTemporaryResults(List<Tuple<object, int>> temporaryResults)
        {
            return temporaryResults.First().Item1;
        }
    }


    [SamplingMethod("plusone")]
    public class DayOfWeekSampling : SamplingImplementationBase
    {
        /// <summary>
        /// Implementation method must be static
        /// </summary>
        public static double PlusOne(double value)
        {
            return value + 1;
        }

        public override MethodInfo GetSamplingProcessingMethod(Type genericType)
        {
            return this.GetType().GetMethod("PlusOne");
        }

        public override Type GetResultType(Type inputType)
        {
            return typeof(double);
        }
    }

    public class ExternalMethodsHandlerTests
    {
        [Scenario]
        public void RegisterExternalMethodFromLoaclFile()
        {
            var name = Assembly.GetExecutingAssembly().GetName().Name + ".dll";
            var path = Environment.CurrentDirectory;
            var realPath = Path.Combine(path, name);

            ExternalMethodsHandler handler = null;
            int numberOfRegisteredMethods = 0;
            "".Given(() => handler = new ExternalMethodsHandler() {RemoteFileUri = new Uri(realPath)});
            "".Then(() => numberOfRegisteredMethods = handler.RegisterExternalMethods());
            "".Then(
                () =>
                    handler.AggregationMethodsAssembly.GetName().Name
                        .Should()
                        .Be(Assembly.GetExecutingAssembly().GetName().Name));
            "".Then(() => numberOfRegisteredMethods.Should().Be(2));

        }


        [Scenario]
         public void RegisterExternalMethodFromWeb()
        {
            string externalAssemblyUri = @"https://manupoc.blob.core.windows.net/code/CustomAggregationMethods.dll";

            ExternalMethodsHandler handler = null;
            int numberOfRegisteredMethods = 0;
            "".Given(() => handler = new ExternalMethodsHandler() {RemoteFileUri = new Uri(externalAssemblyUri)});
            "".Then(() => numberOfRegisteredMethods = handler.RegisterExternalMethods());
            "".Then(
                () =>
                    handler.AggregationMethodsAssembly.GetName().Name
                        .Should()
                        .Be("CustomAggregationMethods"));
            "".Then(() => numberOfRegisteredMethods.Should().Be(2));

        }

        [Scenario]
        public void RegisterExternalMethodFromWebWrongAddress()
        {
            Exception exception = null;
            string externalAssemblyUri = @"https://manupoc.blob.core.windows.net/yyy/xxx.dll";

            ExternalMethodsHandler handler = null;
            int numberOfRegisteredMethods = 0;
            "".Given(() => handler = new ExternalMethodsHandler() { RemoteFileUri = new Uri(externalAssemblyUri) });
            "".Given(() => exception= Record.Exception(
                            () => numberOfRegisteredMethods = handler.RegisterExternalMethods()));
            "".Then(() => exception.Should().BeOfType<InvalidOperationException>());
            "".Then(() => exception.InnerException.Should().BeOfType<WebException>());
        }

        [Scenario]
        public void RegisterExternalMethodFromWebWrongPath()
        {
            Exception exception = null;
            string externalAssemblyUri = @"c:\xxx.dll";

            ExternalMethodsHandler handler = null;
            int numberOfRegisteredMethods = 0;
            "".Given(() => handler = new ExternalMethodsHandler() { RemoteFileUri = new Uri(externalAssemblyUri) });
            "".Given(() => exception = Record.Exception(
                            () => numberOfRegisteredMethods = handler.RegisterExternalMethods()));
            "".Then(() => exception.Should().BeOfType<ArgumentException>());
            "".Then(() => exception.InnerException.Should().BeOfType<FileNotFoundException>());
        }


        [Scenario]
        public void RegisterExternalMethodFromWebWrongSchema()
        {
            Exception exception = null;
            string externalAssemblyUri = @"tcp://manupoc.blob.core.windows.net/yyy/xxx.dll";

            ExternalMethodsHandler handler = null;
            int numberOfRegisteredMethods = 0;
            "".Given(() => handler = new ExternalMethodsHandler() { RemoteFileUri = new Uri(externalAssemblyUri) });
            "".Given(() => exception = Record.Exception(
                            () => numberOfRegisteredMethods = handler.RegisterExternalMethods()));
            "".Then(() => exception.Should().BeOfType<ArgumentException>());
        }

        [Scenario]
        public void RegisterExternalMethodFromWebWrongResourceName()
        {
            Exception exception = null;
            string externalAssemblyUri = @"c:\xxx.yyy";

            ExternalMethodsHandler handler = null;
            int numberOfRegisteredMethods = 0;
            "".Given(() => handler = new ExternalMethodsHandler() { RemoteFileUri = new Uri(externalAssemblyUri) });
            "".Given(() => exception = Record.Exception(
                            () => numberOfRegisteredMethods = handler.RegisterExternalMethods()));
            "".Then(() => exception.Should().BeOfType<ArgumentException>());
        }


        [Scenario]
        public void RegisterExternalMethodFromWebNullPath()
        {

            ExternalMethodsHandler handler = null;
            int numberOfRegisteredMethods = 0;
            "".Given(() => handler = new ExternalMethodsHandler());
            "".Then(() => numberOfRegisteredMethods = handler.RegisterExternalMethods());
            "".Then(() => numberOfRegisteredMethods.Should().Be(0));

        }

        
    }
}
