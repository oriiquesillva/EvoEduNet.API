using System;
using System.Data;
using System.Threading.Tasks;
using EvoEduNet.API.Domain.Dtos;
using EvoEduNet.API.Domain.Entities;
using EvoEduNet.API.Domain.Exceptions;
using EvoEduNet.API.Infrastructure.Data;
using EvoEduNet.API.Repositories.Interfaces;
using EvoEduNet.API.Services.Interfaces;

namespace EvoEduNet.API.Services
{
    public class MatriculaService : IMatriculaService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IAlunoRepository _alunoRepository;
        private readonly ITurmaRepository _turmaRepository;
        private readonly IMatriculaRepository _matriculaRepository;
        private readonly ITurmaCacheService _turmaCacheService;

        public MatriculaService(
            IDbConnectionFactory connectionFactory,
            IAlunoRepository alunoRepository,
            ITurmaRepository turmaRepository,
            IMatriculaRepository matriculaRepository,
            ITurmaCacheService turmaCacheService)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _alunoRepository = alunoRepository ?? throw new ArgumentNullException(nameof(alunoRepository));
            _turmaRepository = turmaRepository ?? throw new ArgumentNullException(nameof(turmaRepository));
            _matriculaRepository = matriculaRepository ?? throw new ArgumentNullException(nameof(matriculaRepository));
            _turmaCacheService = turmaCacheService ?? throw new ArgumentNullException(nameof(turmaCacheService));
        }

        public async Task<MatriculaResponseDto> RealizarMatriculaAsync(CriarMatriculaDto dto)
        {
            if (dto == null)
            {
                throw new ValidationException("Os dados da matrícula são obrigatórios.");
            }

            // 1. Validação de Aluno: existência e status ativo
            var aluno = await _alunoRepository.ObterPorIdAsync(dto.AlunoId);
            if (aluno == null)
            {
                throw new NotFoundException($"Aluno com ID {dto.AlunoId} não foi encontrado.");
            }

            if (!aluno.Ativo)
            {
                throw new BusinessException($"O aluno '{aluno.Nome}' está inativo e não pode realizar matrículas.");
            }

            // 2. Abertura da Conexão e Transação Atômica ACID
            using (var connection = _connectionFactory.CreateConnection())
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        // 3. Validação de Turma: existência e vagas dentro da transação
                        var turma = await _turmaRepository.ObterPorIdAsync(dto.TurmaId, transaction);
                        if (turma == null)
                        {
                            throw new NotFoundException($"Turma com ID {dto.TurmaId} não foi encontrada.");
                        }

                        if (turma.VagasDisponiveis <= 0)
                        {
                            throw new BusinessException($"A turma '{turma.Nome}' não possui vagas disponíveis.");
                        }

                        // 4. Validação de Duplicidade: aluno não pode estar na mesma turma duas vezes
                        var jaMatriculado = await _matriculaRepository.ExisteMatriculaAsync(dto.AlunoId, dto.TurmaId, transaction);
                        if (jaMatriculado)
                        {
                            throw new BusinessException($"O aluno '{aluno.Nome}' já está matriculado na turma '{turma.Nome}'.");
                        }

                        // 5. Inserção do registro de matrícula
                        var matricula = new Matricula
                        {
                            AlunoId = dto.AlunoId,
                            TurmaId = dto.TurmaId,
                            DataMatricula = DateTime.Now
                        };

                        var matriculaId = await _matriculaRepository.InserirAsync(matricula, transaction);

                        // 6. Decremento de vagas com validação de concorrência
                        var decrementou = await _turmaRepository.DecrementarVagasAsync(dto.TurmaId, transaction);
                        if (!decrementou)
                        {
                            throw new BusinessException($"As vagas da turma '{turma.Nome}' esgotaram-se durante o processamento.");
                        }

                        // 7. Confirmação atômica da transação (Commit)
                        transaction.Commit();

                        // 8. Invalidação do Cache de Turmas (Bônus)
                        await _turmaCacheService.InvalidarAsync();

                        return new MatriculaResponseDto
                        {
                            Id = matriculaId,
                            AlunoId = aluno.Id,
                            NomeAluno = aluno.Nome,
                            TurmaId = turma.Id,
                            NomeTurma = turma.Nome,
                            DataMatricula = matricula.DataMatricula,
                            Mensagem = $"Matrícula realizada com sucesso para o aluno '{aluno.Nome}' na turma '{turma.Nome}'."
                        };
                    }
                    catch
                    {
                        // Rollback em qualquer falha de validação ou de banco
                        try
                        {
                            transaction.Rollback();
                        }
                        catch
                        {
                            // Transação já finalizada ou conexão encerrada
                        }

                        throw;
                    }
                }
            }
        }
    }
}
