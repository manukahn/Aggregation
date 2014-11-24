using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.OData;
using System.Web.OData.Query;
using TestODataService.Models;
using Microsoft.Data.OData;


namespace TestODataService.Controllers
{
    public class SalesController : ODataController
    {
        
        private List<Sales> sales = new List<Sales>();

        private void CreateSales()
        {
            if (sales.Count == 0)
            {
                Category c1 = new Category() { CategoryIdentifier = 1, Name = "Sports", CategoryTime = DateTime.Now.AddDays(-1)};
                Category c2 = new Category() { CategoryIdentifier = 2, Name = "Hardware", CategoryTime = DateTime.Now };

                var p1 = new Product() { ProductIdentifier = 1, Category = c1, ProductName = "Football", Color = "White", TaxRate = 20.4, ProdctionTime = DateTime.Now.AddDays(-100)};
                var p2 = new Product() { ProductIdentifier = 1, Category = c2, ProductName = "Hammer", Color = "Black", TaxRate = 25.0, ProdctionTime = DateTime.Now.AddDays(-200) };
                var p3 = new Product() { ProductIdentifier = 1, Category = null, ProductName = "Hammer", Color = "Black", TaxRate = 25.0, ProdctionTime = DateTime.Now.AddDays(-200) };



                sales.Add(new Sales() { Id = 1, Amount = 100, Product = p1, Products = new Product[]{p1,p2,p3},  Time = DateTime.Now.AddDays(-1)});
                sales.Add(new Sales() { Id = 11, Amount = 100, Product = p1, Products = new Product[] { p1, p2, p3 }, Time = DateTime.Now });
                sales.Add(new Sales() { Id = 111, Amount = 100, Product = p1, Products = new Product[] { p1, p2, p3 }, Time = DateTime.Now });
                sales.Add(new Sales() { Id = 1111, Amount = 100, Product = p1, Products = new Product[] { p1, p2, p3 }, Time = DateTime.Now.AddDays(-1) });
                sales.Add(new Sales() { Id = 11111, Amount = 100, Product = p1, Products = new Product[] { p1, p2, p3 }, Time = DateTime.Now });
                sales.Add(new Sales() { Id = 111111, Amount = 100, Product = p1, Products = new Product[] { p1, p2, p3 }, Time = DateTime.Now });
                sales.Add(new Sales() { Id = 2, Amount = 20, Product = p2, Products = new Product[] { p1, p2, p3 }, Time = DateTime.Now.AddDays(-2) });
                sales.Add(new Sales() { Id = 3, Amount = 30, Product = p2, Products = new Product[] { p1, p2, p3 }, Time = DateTime.Now });
                sales.Add(new Sales() { Id = 4, Amount = 40, Product = p1, Products = new Product[] { p1, p2, p3 }, Time = DateTime.Now });
                sales.Add(new Sales() { Id = 5, Amount = 50, Product = p2, Products = new Product[] { p1, p2, p3 }, Time = DateTime.Now.AddDays(-3) });

                MongoDal.CreateGraph(sales);


                //Test null propagation
                //sales.Add(new Sales() { Id = 6, Amount = 50, Product = p3, Time = DateTime.Now.AddDays(-3) });
                //sales.Add(new Sales() { Id = 7, Amount = 50, Product = null, Time = DateTime.Now.AddDays(-3) });
                //sales.Add(new Sales());
            }
        }



        // GET: odata/Sales
        //[EnableQuery(PageSize = 20)]
        //public IQueryable<Sales> Get(ODataQueryOptions<Sales> queryOptions)
        //{
        //    CreateSales();

        //    return sales.AsQueryable();
        //}


        [EnableQuery(PageSize = 20)]
        public IHttpActionResult Get()
        {
            //CreateSales();
            //return Ok(sales.AsQueryable());

            return Ok(MongoDal.GetGraph());
        }

        [EnableQuery(PageSize = 20)]
        [HttpGet]
        public IHttpActionResult Get([FromODataUri]int id)
        {
            return Ok(MongoDal.GetSale(id));
        }

        
 
    }

}
