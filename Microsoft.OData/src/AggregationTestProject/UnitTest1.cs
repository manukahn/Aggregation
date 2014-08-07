using System;
using System.CodeDom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.OData.Core.Aggregation;

namespace AggregationTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void AppltyParseTest()
        {
            //string query = "?apply=aggregate(Amount mul Product/TaxRate with sum as Tax)/groupby(jkdjklelgjg)/aggregate(kfhlwejs)/groupby(hfwkjwmns)";
            //var res = ApplyParser.ParseApplyImplementation(query);
            //Assert.IsNotNull(res);


            //string str = "hello(1234))";
            //var res = str.TrimOne('(', ')');
            //Console.WriteLine(res);


            //string str = "round(product";
            //var res = str.TrimMethodCallPrefix();
            //Console.WriteLine(res);


            string str = "taxRate)";
            var res = str.TrimMethodCallSufix();
            Console.WriteLine(res);

        }
    }
}
