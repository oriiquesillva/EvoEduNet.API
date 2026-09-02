using System.Net.Http.Headers;
using System.Web.Http;
using EvoEduNet.API.Controllers;
using EvoEduNet.API.Infrastructure.Cache;
using EvoEduNet.API.Infrastructure.Data;
using EvoEduNet.API.Infrastructure.Filters;
using EvoEduNet.API.Infrastructure.IoC;
using EvoEduNet.API.Repositories;
using EvoEduNet.API.Repositories.Interfaces;
using EvoEduNet.API.Services;
using EvoEduNet.API.Services.Interfaces;
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

            // 1. Injeção de Dependências (IoC) com SimpleDependencyResolver
            var resolver = new SimpleDependencyResolver();

            // Infraestrutura
            IDbConnectionFactory connectionFactory = new SqlConnectionFactory("DefaultConnection");
            ITurmaCacheService cacheService = new MemoryTurmaCacheService();

            // Repositórios Dapper
            var alunoRepo = new AlunoRepository(connectionFactory);
            var turmaRepo = new TurmaRepository(connectionFactory);
            var matriculaRepo = new MatriculaRepository(connectionFactory);
            var relatorioRepo = new RelatorioRepository(connectionFactory);

            // Serviços de Regras de Negócio
            var alunoService = new AlunoService(alunoRepo);
            var turmaService = new TurmaService(turmaRepo, cacheService);
            var matriculaService = new MatriculaService(connectionFactory, alunoRepo, turmaRepo, matriculaRepo, cacheService);
            var relatorioService = new RelatorioService(relatorioRepo);

            // Registro de Interfaces
            resolver.Register<IDbConnectionFactory>(() => connectionFactory);
            resolver.Register<ITurmaCacheService>(() => cacheService);
            resolver.Register<IAlunoRepository>(() => alunoRepo);
            resolver.Register<ITurmaRepository>(() => turmaRepo);
            resolver.Register<IMatriculaRepository>(() => matriculaRepo);
            resolver.Register<IRelatorioRepository>(() => relatorioRepo);
            resolver.Register<IAlunoService>(() => alunoService);
            resolver.Register<ITurmaService>(() => turmaService);
            resolver.Register<IMatriculaService>(() => matriculaService);
            resolver.Register<IRelatorioService>(() => relatorioService);

            // Registro de Controllers
            resolver.Register<AlunosController>(() => new AlunosController(alunoService));
            resolver.Register<TurmasController>(() => new TurmasController(turmaService));
            resolver.Register<MatriculasController>(() => new MatriculasController(matriculaService));
            resolver.Register<RelatoriosController>(() => new RelatoriosController(relatorioService));
            resolver.Register<StatusController>(() => new StatusController());

            config.DependencyResolver = resolver;

            // 2. Habilitar CORS para permitir consumo do front-end
            app.UseCors(CorsOptions.AllowAll);

            // 3. Configuração de Rotas
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // 4. Configuração de Serialização JSON (CamelCase e formatação limpa)
            var jsonFormatter = config.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            jsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            jsonFormatter.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
            
            // Forçar retorno em JSON quando acessado pelo navegador
            jsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            // 5. Registro de Filtros Globais (Validação de ModelState e Tratamento de Exceções)
            config.Filters.Add(new ValidateModelAttribute());
            config.Filters.Add(new CustomExceptionFilterAttribute());

            // 6. Acoplar configuração ao pipeline OWIN
            app.UseWebApi(config);
        }
    }
}
