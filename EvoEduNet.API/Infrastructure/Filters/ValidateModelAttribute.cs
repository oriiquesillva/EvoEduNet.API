using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace EvoEduNet.API.Infrastructure.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!actionContext.ModelState.IsValid)
            {
                var erros = actionContext.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => !string.IsNullOrWhiteSpace(e.ErrorMessage) ? e.ErrorMessage : e.Exception?.Message)
                    .Where(msg => !string.IsNullOrWhiteSpace(msg))
                    .ToList();

                var responseObj = new
                {
                    mensagem = "Dados da requisição inválidos.",
                    erros = erros
                };

                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest, responseObj);
            }
        }
    }
}
