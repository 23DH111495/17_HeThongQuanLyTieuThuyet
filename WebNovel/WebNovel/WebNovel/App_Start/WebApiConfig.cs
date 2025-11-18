using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;


namespace WebNovel.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Enable CORS for mobile app
            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);
    
            // Attribute routing
            config.MapHttpAttributeRoutes();

            // Convention routing
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Remove XML formatter, keep only JSON
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // JSON formatting
            config.Formatters.JsonFormatter.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}