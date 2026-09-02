using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EvoEduNet.API.Domain.Dtos;
using EvoEduNet.API.Domain.Entities;
using EvoEduNet.API.Domain.Exceptions;
using EvoEduNet.API.Repositories.Interfaces;
using EvoEduNet.API.Services.Interfaces;

namespace EvoEduNet.API.Services
{
    public class AlunoService : IAlunoService
    {
        private readonly IAlunoRepository _alunoRepository;

        public AlunoService(IAlunoRepository alunoRepository)
        {
            _alunoRepository = alunoRepository ?? throw new ArgumentNullException(nameof(alunoRepository));
        }

        public async Task<PagedResultDto<AlunoResponseDto>> ObterPaginadoAsync(string nome, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var resultado = await _alunoRepository.ObterPaginadoAsync(nome, page, pageSize);

            var dtos = resultado.Item1.Select(MapToResponseDto);

            return new PagedResultDto<AlunoResponseDto>(dtos, resultado.Item2, page, pageSize);
        }

        public async Task<AlunoResponseDto> ObterPorIdAsync(int id)
        {
            var aluno = await _alunoRepository.ObterPorIdAsync(id);
            if (aluno == null)
            {
                throw new NotFoundException($"Aluno com ID {id} não foi encontrado.");
            }

            return MapToResponseDto(aluno);
        }

        public async Task<IEnumerable<AlunoResponseDto>> ObterTodosAsync(bool apenasAtivos = true)
        {
            var alunos = await _alunoRepository.ObterTodosAsync(apenasAtivos);
            return alunos.Select(MapToResponseDto);
        }

        public async Task<AlunoResponseDto> CriarAsync(CriarAlunoDto dto)
        {
            if (dto == null)
            {
                throw new ValidationException("Os dados do aluno devem ser fornecidos.");
            }

            var emailFormatado = dto.Email.Trim().ToLowerInvariant();

            // Validação de unicidade de e-mail (regra de negócio)
            var alunoExistente = await _alunoRepository.ObterPorEmailAsync(emailFormatado);
            if (alunoExistente != null)
            {
                throw new BusinessException($"Já existe um aluno cadastrado com o e-mail '{dto.Email}'.");
            }

            var novoAluno = new Aluno
            {
                Nome = dto.Nome.Trim(),
                Email = emailFormatado,
                DataNascimento = dto.DataNascimento.Value,
                Ativo = true,
                DataCadastro = DateTime.Now
            };

            novoAluno.Id = await _alunoRepository.InserirAsync(novoAluno);

            return MapToResponseDto(novoAluno);
        }

        public async Task<AlunoResponseDto> AtualizarAsync(int id, AtualizarAlunoDto dto)
        {
            if (dto == null)
            {
                throw new ValidationException("Os dados para atualização do aluno devem ser fornecidos.");
            }

            var aluno = await _alunoRepository.ObterPorIdAsync(id);
            if (aluno == null)
            {
                throw new NotFoundException($"Aluno com ID {id} não foi encontrado.");
            }

            var emailFormatado = dto.Email.Trim().ToLowerInvariant();

            // Validar se o novo e-mail já está em uso por outro aluno
            var alunoPorEmail = await _alunoRepository.ObterPorEmailAsync(emailFormatado);
            if (alunoPorEmail != null && alunoPorEmail.Id != id)
            {
                throw new BusinessException($"O e-mail '{dto.Email}' já está sendo utilizado por outro aluno.");
            }

            aluno.Nome = dto.Nome.Trim();
            aluno.Email = emailFormatado;
            aluno.DataNascimento = dto.DataNascimento.Value;
            if (dto.Ativo.HasValue)
            {
                aluno.Ativo = dto.Ativo.Value;
            }

            var atualizado = await _alunoRepository.AtualizarAsync(aluno);
            if (!atualizado)
            {
                throw new BusinessException($"Não foi possível atualizar o aluno com ID {id}.");
            }

            return MapToResponseDto(aluno);
        }

        public async Task ExcluirLogicoAsync(int id)
        {
            var aluno = await _alunoRepository.ObterPorIdAsync(id);
            if (aluno == null)
            {
                throw new NotFoundException($"Aluno com ID {id} não foi encontrado.");
            }

            // Se já estiver inativo, não quebra a operação (idempotência)
            if (!aluno.Ativo)
            {
                return;
            }

            var excluido = await _alunoRepository.ExcluirLogicoAsync(id);
            if (!excluido)
            {
                throw new BusinessException($"Não foi possível inativar o aluno com ID {id}.");
            }
        }

        private static AlunoResponseDto MapToResponseDto(Aluno aluno)
        {
            return new AlunoResponseDto
            {
                Id = aluno.Id,
                Nome = aluno.Nome,
                Email = aluno.Email,
                DataNascimento = aluno.DataNascimento,
                Ativo = aluno.Ativo,
                DataCadastro = aluno.DataCadastro
            };
        }
    }
}
