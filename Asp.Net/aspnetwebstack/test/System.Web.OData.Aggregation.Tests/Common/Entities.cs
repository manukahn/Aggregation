using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.OData.Aggregation.Tests.Common
{

    public class Sales
    {
        public int Id { get; set; }

        public Product Product { get; set; }

        public Product[] Products { get; set; }

        public double Amount { get; set; }

        public DateTimeOffset Time { get; set; }

        public static int DummyInt(Sales s)
        {
            return 5;
        }

        public static string DummyString(string s)
        {
            return "5";
        }

    }

    public class Product
    {
        public int ProductIdentifier { get; set; }

        public Category Category { get; set; }

        public string ProductName { get; set; }

        public string Color { get; set; }

        public double TaxRate { get; set; }

    }

    public class Category
    {
        public int CategoryIdentifier { get; set; }

        public string Name { get; set; }

    }

    public static class TestDataSource
    {
        public static IQueryable CreateData()
        {
            var sales = new List<Sales>();
            Category c1 = new Category() { CategoryIdentifier = 1, Name = "Sports", };
            Category c2 = new Category() { CategoryIdentifier = 2, Name = "Hardware",  };

            var p1 = new Product() { ProductIdentifier = 1, Category = c1, ProductName = "Football", Color = "White", TaxRate = 20.4 };
            var p2 = new Product() { ProductIdentifier = 1, Category = c2, ProductName = "Hammer", Color = "Black", TaxRate = 25.0 };
            var p3 = new Product() { ProductIdentifier = 1, Category = null, ProductName = "Hammer", Color = "Black", TaxRate = 25.0 };



            sales.Add(new Sales() { Id = 1, Amount = 100, Product = p1, Products = new Product[] { p1, p2, p3 }, Time = DateTime.Now.AddDays(-1) });
            sales.Add(new Sales() { Id = 11, Amount = 100, Product = p1, Products = new Product[] { p1, p2, p3 }, Time = DateTime.Now });
            sales.Add(new Sales() { Id = 111, Amount = 100, Product = p1, Products = new Product[] { p1, p2, p3 }, Time = DateTime.Now });
            sales.Add(new Sales() { Id = 2, Amount = 20, Product = p1, Products = new Product[] { p1, p2, p3 }, Time = DateTime.Now.AddDays(-2) });
            sales.Add(new Sales() { Id = 3, Amount = 30, Product = p1, Products = new Product[] { p1, p2, p3 }, Time = DateTime.Now });
            sales.Add(new Sales() { Id = 4, Amount = 40, Product = p1, Products = new Product[] { p1, p2, p3 }, Time = DateTime.Now });
            sales.Add(new Sales() { Id = 5, Amount = 50, Product = p1, Products = new Product[] { p1, p2, p3 }, Time = DateTime.Now.AddDays(-3) });

            return sales.AsQueryable();
        }

        
    }
}
