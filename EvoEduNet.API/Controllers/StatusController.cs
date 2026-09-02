using System;
using System.Web.Http;

namespace EvoEduNet.API.Controllers
{
    [RoutePrefix("api/status")]
    public class StatusController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetStatus()
        {
            return Ok(new
            {
                status = "Online",
                projeto = "EvoEduNet.API",
                versao = "1.0.0",
                plataforma = ".NET Framework 4.8",
                dataHora = DateTime.Now
            });
        }
    }
}
