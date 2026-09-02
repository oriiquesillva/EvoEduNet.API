using System;
using System.Threading.Tasks;
using System.Web.Http;
using EvoEduNet.API.Services.Interfaces;

namespace EvoEduNet.API.Controllers
{
    [RoutePrefix("api/turmas")]
    public class TurmasController : ApiController
    {
        private readonly ITurmaService _turmaService;

        public TurmasController(ITurmaService turmaService)
        {
            _turmaService = turmaService ?? throw new ArgumentNullException(nameof(turmaService));
        }

        /// <summary>
        /// Lista turmas escolares com exibição das vagas restantes.
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> ObterTodas()
        {
            var turmas = await _turmaService.ObterTodasAsync();
            return Ok(turmas);
        }

        /// <summary>
        /// Busca turma específica por ID.
        /// </summary>
        [HttpGet]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> ObterPorId(int id)
        {
            var turma = await _turmaService.ObterPorIdAsync(id);
            return Ok(turma);
        }
    }
}
