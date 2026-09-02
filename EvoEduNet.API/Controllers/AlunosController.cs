using System;
using System.Threading.Tasks;
using System.Web.Http;
using EvoEduNet.API.Domain.Dtos;
using EvoEduNet.API.Services.Interfaces;

namespace EvoEduNet.API.Controllers
{
    [RoutePrefix("api/alunos")]
    public class AlunosController : ApiController
    {
        private readonly IAlunoService _alunoService;

        public AlunosController(IAlunoService alunoService)
        {
            _alunoService = alunoService ?? throw new ArgumentNullException(nameof(alunoService));
        }

        /// <summary>
        /// Listagem paginada de alunos com filtro opcional por nome e total de registros.
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> ObterPaginado([FromUri] string nome = null, [FromUri] int page = 1, [FromUri] int pageSize = 10)
        {
            var resultado = await _alunoService.ObterPaginadoAsync(nome, page, pageSize);
            return Ok(resultado);
        }

        /// <summary>
        /// Retorna todos os alunos (sem paginação). Por padrão retorna apenas os ativos, com opção de listar todos incluindo inativos.
        /// </summary>
        [HttpGet]
        [Route("todos")]
        public async Task<IHttpActionResult> ObterTodos([FromUri] bool apenasAtivos = true)
        {
            var alunos = await _alunoService.ObterTodosAsync(apenasAtivos);
            return Ok(alunos);
        }

        /// <summary>
        /// Busca por ID do aluno.
        /// </summary>
        [HttpGet]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> ObterPorId(int id)
        {
            var aluno = await _alunoService.ObterPorIdAsync(id);
            return Ok(aluno);
        }

        /// <summary>
        /// Cadastro de um novo aluno.
        /// </summary>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Criar([FromBody] CriarAlunoDto dto)
        {
            if (dto == null)
            {
                return BadRequest("O corpo da requisição não pode ser vazio.");
            }

            var novoAluno = await _alunoService.CriarAsync(dto);
            return Created($"api/alunos/{novoAluno.Id}", novoAluno);
        }

        /// <summary>
        /// Atualização dos dados cadastrais do aluno.
        /// </summary>
        [HttpPut]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Atualizar(int id, [FromBody] AtualizarAlunoDto dto)
        {
            if (dto == null)
            {
                return BadRequest("O corpo da requisição não pode ser vazio.");
            }

            var alunoAtualizado = await _alunoService.AtualizarAsync(id, dto);
            return Ok(alunoAtualizado);
        }

        /// <summary>
        /// Exclusão lógica do aluno (altera Ativo para 0).
        /// </summary>
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Excluir(int id)
        {
            await _alunoService.ExcluirLogicoAsync(id);
            return Ok(new { mensagem = $"Aluno com ID {id} inativado com sucesso." });
        }
    }
}
