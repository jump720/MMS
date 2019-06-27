using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Routing;

namespace MMS
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings =
                new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Local
                };

            config.MapHttpAttributeRoutes();

            RouteTable.Routes.MapHttpRoute(
                name: "PlantillasApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
                ).RouteHandler = new MyHttpControllerRouteHandler();

            RouteTable.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                ).RouteHandler = new MyHttpControllerRouteHandler();

            //    RouteTable.Routes.MapHttpRoute(
            //        name: "DefaultApi",
            //        routeTemplate: "api/{controller}/{action}/{id}",
            //        defaults: new { id = RouteParameter.Optional }
            //    ).RouteHandler = new SessionRouteHandler();


            //    var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            //    config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);
            //    //var json = config.Formatters.JsonFormatter;
            //    //json.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
            //    //config.Formatters.Remove(config.Formatters.XmlFormatter);
            //    //var formatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            //    //formatter.SerializerSettings.ContractResolver =
            //    //    new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

        }

        public class MyHttpControllerHandler : System.Web.Http.WebHost.HttpControllerHandler, System.Web.SessionState.IRequiresSessionState
        {
            public MyHttpControllerHandler(RouteData routeData) : base(routeData)
            { }
        }

        public class MyHttpControllerRouteHandler : System.Web.Http.WebHost.HttpControllerRouteHandler
        {
            protected override System.Web.IHttpHandler GetHttpHandler(RequestContext requestContext)
            {
                return new MyHttpControllerHandler(requestContext.RouteData);
            }
        }

    }
}