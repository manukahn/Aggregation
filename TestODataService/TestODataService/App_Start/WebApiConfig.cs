using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.UI.WebControls;
using Microsoft.OData.Core.Aggregation;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using TestODataService.Models;

namespace TestODataService
{
    public static class WebApiConfig
    {
        
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.MapODataServiceRoute("odata", "odata", model: GetModel());

            config.EnableQuerySupport();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = System.Web.Http.RouteParameter.Optional }
            );
        }

        public static Microsoft.OData.Edm.IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            var set =  builder.EntitySet<Sales>("Sales");
            builder.AddEntityType(typeof(Product));
            var model = builder.GetEdmModel();
            return model;
        }
    }
}
