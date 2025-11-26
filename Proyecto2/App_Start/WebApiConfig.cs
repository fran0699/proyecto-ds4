using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Proyecto2
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // habilitación de rutas por atributos [Route]
            config.MapHttpAttributeRoutes();

            // Ruta por defecto de Web API
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
