using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using WebApi.MessagePack.App_Start;

namespace WebApi.MessagePack
{
    public class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {

            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            config.Filters.Add(new WebApiTrackerAttribute());

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Formatters.Add(new ProtoBufFormatter());
            config.Formatters.Add(new MessagePackFormatter());

            config.Formatters.Add(new ProductCsvFormatter());
            var bson = new BsonMediaTypeFormatter();
            bson.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.contoso"));
            config.Formatters.Add(bson);
            appBuilder.UseWebApi(config);
        }
    }
}
