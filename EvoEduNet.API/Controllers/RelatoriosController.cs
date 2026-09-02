using System;
using System.Threading.Tasks;
using System.Web.Http;
using EvoEduNet.API.Services.Interfaces;

namespace EvoEduNet.API.Controllers
{
    [RoutePrefix("api/relatorios")]
    public class RelatoriosController : ApiController
    {
        private readonly IRelatorioService _relatorioService;

        public RelatoriosController(IRelatorioService relatorioService)
        {
            _relatorioService = relatorioService ?? throw new ArgumentNullException(nameof(relatorioService));
        }

        /// <summary>
        /// Relatório analítico agregado de alunos por turma (SQL nativo JOIN + GROUP BY).
        /// </summary>
        [HttpGet]
        [Route("alunos-por-turma")]
        public async Task<IHttpActionResult> ObterAlunosPorTurma()
        {
            var relatorio = await _relatorioService.ObterAlunosPorTurmaAsync();
            return Ok(relatorio);
        }
    }
}
