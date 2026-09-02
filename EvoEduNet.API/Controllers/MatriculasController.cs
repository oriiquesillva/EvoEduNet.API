using System;
using System.Threading.Tasks;
using System.Web.Http;
using EvoEduNet.API.Domain.Dtos;
using EvoEduNet.API.Services.Interfaces;

namespace EvoEduNet.API.Controllers
{
    [RoutePrefix("api/matriculas")]
    public class MatriculasController : ApiController
    {
        private readonly IMatriculaService _matriculaService;

        public MatriculasController(IMatriculaService matriculaService)
        {
            _matriculaService = matriculaService ?? throw new ArgumentNullException(nameof(matriculaService));
        }

        /// <summary>
        /// Realiza a matrícula do aluno em uma turma com transação atômica ACID.
        /// Retorna 201 Created em caso de sucesso ou 409 Conflict se alguma regra for violada.
        /// </summary>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> RealizarMatricula([FromBody] CriarMatriculaDto dto)
        {
            if (dto == null)
            {
                return BadRequest("O corpo da requisição não pode ser vazio.");
            }

            var matricula = await _matriculaService.RealizarMatriculaAsync(dto);
            return Created($"api/matriculas/{matricula.Id}", matricula);
        }
    }
}
