using System.Net.Http.Headers;
using System.Web.Http;
using EvoEduNet.API.Infrastructure.Filters;
using Microsoft.Owin.Cors;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;

namespace EvoEduNet.API
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            // 1. Habilitar CORS para permitir consumo do front-end
            app.UseCors(CorsOptions.AllowAll);

            // 2. Configuração de Rotas
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // 3. Configuração de Serialização JSON (CamelCase e formatação limpa)
            var jsonFormatter = config.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            jsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            jsonFormatter.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
            
            // Forçar retorno em JSON quando acessado pelo navegador
            jsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            // 4. Registro de Filtros Globais (Validação de ModelState e Tratamento de Exceções)
            config.Filters.Add(new ValidateModelAttribute());
            config.Filters.Add(new CustomExceptionFilterAttribute());

            // 5. Acoplar configuração ao pipeline OWIN
            app.UseWebApi(config);
        }
    }
}
