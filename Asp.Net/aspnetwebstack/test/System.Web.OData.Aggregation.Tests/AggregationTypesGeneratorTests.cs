using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.OData.Aggregation.Tests.Common;
using System.Web.OData.OData.Query;
using System.Web.OData.Routing;
using Microsoft.OData.Edm.Library;
using Xbehave;
using Xunit;
using Xunit.Extensions;
using FluentAssertions;

namespace System.Web.OData.Aggregation.Tests
{

    public class SomeType
    {
        public string Data { get; set; }
    }

    public class AggregationTypesGeneratorTests
    {


        private static ODataQueryContext _context;


        public ODataQueryContext Context
        {
            get
            {
                if (_context == null)
                {
                    var model = Common.TestModelBuilder.CreateModel(new Type[] { typeof(Category), typeof(Product), typeof(Sales) });
                    _context = new ODataQueryContext(model, typeof(Sales), new ODataPath(new ODataPathSegment[]{new EntitySetPathSegment("Sales")}));
                }
                return _context;
            } 
        }


        public static TheoryDataSet<List<Tuple<Type, string>>> GetValidPropertiesOfEntityTypeToCreate
        {
            get
            {
                return new TheoryDataSet<List<Tuple<Type,string>>>()
                {
                    new List<Tuple<Type, string>>(){
                        new Tuple<Type, string>(typeof(string), "Name"),
                        new Tuple<Type, string>(typeof(int), "Age"),
                        new Tuple<Type, string>(typeof(bool), "Prop1"),
                        //new Tuple<Type, string>(typeof(DateTime), "Prop2"),
                        new Tuple<Type, string>(typeof(DateTimeOffset), "Prop3"),
                        new Tuple<Type, string>(typeof(Decimal), "Prop4"),
                        new Tuple<Type, string>(typeof(double), "Prop5"),
                        new Tuple<Type, string>(typeof(long), "Prop6"),
                        new Tuple<Type, string>(typeof(short), "Prop7"),
                        new Tuple<Type, string>(typeof(sbyte), "Prop8"),
                        new Tuple<Type, string>(typeof(Single), "Prop9"),
                        //new Tuple<Type, string>(typeof(SomeType), "MySomeType")
                    }
                };
            }
        }
        

        public static TheoryDataSet<List<Tuple<Type, string>>> GetValidPropertiesOfComplexTypeToCreate
        {
            get
            {
                return new TheoryDataSet<List<Tuple<Type, string>>>()
                {
                    new List<Tuple<Type, string>>(){
                        new Tuple<Type, string>(typeof(string), "Name"),
                        new Tuple<Type, string>(typeof(int), "Age"),
                        new Tuple<Type, string>(typeof(bool), "Prop1"),
                        //new Tuple<Type, string>(typeof(DateTime), "Prop2"),
                        new Tuple<Type, string>(typeof(DateTimeOffset), "Prop3"),
                        new Tuple<Type, string>(typeof(Decimal), "Prop4"),
                        new Tuple<Type, string>(typeof(double), "Prop5"),
                        new Tuple<Type, string>(typeof(long), "Prop6"),
                        new Tuple<Type, string>(typeof(short), "Prop7"),
                        new Tuple<Type, string>(typeof(sbyte), "Prop8"),
                        new Tuple<Type, string>(typeof(Single), "Prop9"),
                        new Tuple<Type, string>(typeof(double), "MySomeType")
                    }
                };
            }
        }


        public static TheoryDataSet<List<Tuple<Type, string>>> GetInvalidPropertiesTypeToCreate
        {
            get
            {
                return new TheoryDataSet<List<Tuple<Type, string>>>()
                {
                    new List<Tuple<Type, string>>(){ new Tuple<Type, string>(typeof(string), "Name Name Name") }                       
                };
            }
        }



        [Scenario]
        [PropertyData("GetValidPropertiesOfEntityTypeToCreate")]
        public void CreateEntityTypeTest(List<Tuple<Type,string>> properties)
        {
            Type newType = null;
            "When create a dynamic type".When(
                () => { newType = AggregationTypesGenerator.CreateType(properties, Context, true); });
            "Then the new type should exist".Then(() => newType.Should().NotBeNull());
            "Then the new type namespace should be ODataAggregation.DynamicTypes".Then(() => newType.Namespace.Should().BeEquivalentTo("ODataAggregation.DynamicTypes"));
            "Then the new type should have 3 properties".Then(() => newType.GetProperties().Count().Should().Be(11));
            "Then the new type should be added as en entity to the model".Then(() => (Context.Model.FindDeclaredType(newType.FullName) as EdmEntityType).Should().NotBeNull());
        }

        [Scenario]
        [PropertyData("GetValidPropertiesOfComplexTypeToCreate")]
        public void CreateComplexTypeTest(List<Tuple<Type, string>> properties)
        {
            Type newType = null;
            "When create a dynamic type".When(
                () => { newType = AggregationTypesGenerator.CreateType(properties, Context, false); });
            "Then the new type should exist".Then(() => newType.Should().NotBeNull());
            "Then the new type namespace should be ODataAggregation.DynamicTypes".Then(() => newType.Namespace.Should().BeEquivalentTo("ODataAggregation.DynamicTypes"));
            "Then the new type should have 3 properties".Then(() => newType.GetProperties().Count().Should().Be(12));
            "Then the new type should be added as en complex type to the model".Then(() => (Context.Model.FindDeclaredType(newType.FullName) as EdmComplexType).Should().NotBeNull());
            
        }


         [Scenario]
        [PropertyData("GetInvalidPropertiesTypeToCreate")]
        public void CreateComplexWithInvalidPropertiesThrows(List<Tuple<Type, string>> properties)
        {
            Exception exception = null;
            "When create a dynamic type".When(
                () => { exception = Record.Exception(() => AggregationTypesGenerator.CreateType(properties, Context, false)); });

             "Then Invalid Operation Exception is thrown".Then(() => exception.Should().BeOfType<InvalidOperationException>());
         }


    }
}
