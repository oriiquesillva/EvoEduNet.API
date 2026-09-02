using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using EvoEduNet.API.Domain.Exceptions;

namespace EvoEduNet.API.Infrastructure.Filters
{
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var exception = actionExecutedContext.Exception;

            if (exception is NotFoundException notFound)
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(
                    HttpStatusCode.NotFound,
                    new { mensagem = notFound.Message }
                );
                return;
            }

            if (exception is BusinessException business)
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(
                    HttpStatusCode.Conflict,
                    new { mensagem = business.Message }
                );
                return;
            }

            if (exception is ValidationException validation)
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(
                    HttpStatusCode.BadRequest,
                    new { mensagem = validation.Message }
                );
                return;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERRO NÃO TRATADO] {DateTime.Now:yyyy-MM-dd HH:mm:ss}: {exception}");
            Console.ResetColor();

            actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(
                HttpStatusCode.InternalServerError,
                new { mensagem = "Ocorreu um erro interno inesperado no servidor." }
            );
        }
    }
}
