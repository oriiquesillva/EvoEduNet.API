using System;
using System.Data;
using System.Threading.Tasks;
using EvoEduNet.API.Domain.Dtos;
using EvoEduNet.API.Domain.Entities;
using EvoEduNet.API.Domain.Exceptions;
using EvoEduNet.API.Infrastructure.Data;
using EvoEduNet.API.Repositories.Interfaces;
using EvoEduNet.API.Services;
using EvoEduNet.API.Services.Interfaces;
using Moq;
using NUnit.Framework;

namespace EvoEduNet.Tests
{
    [TestFixture]
    public class MatriculaServiceTests
    {
        private Mock<IDbConnectionFactory> _mockConnectionFactory;
        private Mock<IDbConnection> _mockConnection;
        private Mock<IDbTransaction> _mockTransaction;
        private Mock<IAlunoRepository> _mockAlunoRepository;
        private Mock<ITurmaRepository> _mockTurmaRepository;
        private Mock<IMatriculaRepository> _mockMatriculaRepository;
        private Mock<ITurmaCacheService> _mockCacheService;
        private MatriculaService _service;

        [SetUp]
        public void Setup()
        {
            _mockConnectionFactory = new Mock<IDbConnectionFactory>();
            _mockConnection = new Mock<IDbConnection>();
            _mockTransaction = new Mock<IDbTransaction>();
            _mockAlunoRepository = new Mock<IAlunoRepository>();
            _mockTurmaRepository = new Mock<ITurmaRepository>();
            _mockMatriculaRepository = new Mock<IMatriculaRepository>();
            _mockCacheService = new Mock<ITurmaCacheService>();

            // Setup da conexão e transação simuladas
            _mockConnection.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(_mockTransaction.Object);
            _mockConnectionFactory.Setup(f => f.CreateConnection()).Returns(_mockConnection.Object);

            _service = new MatriculaService(
                _mockConnectionFactory.Object,
                _mockAlunoRepository.Object,
                _mockTurmaRepository.Object,
                _mockMatriculaRepository.Object,
                _mockCacheService.Object
            );
        }

        [Test]
        public void RealizarMatricula_QuandoDtoNulo_DeveLancarValidationException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ValidationException>(() => _service.RealizarMatriculaAsync(null));
            Assert.That(ex.Message, Does.Contain("Os dados da matrícula são obrigatórios."));
        }

        [Test]
        public void RealizarMatricula_QuandoAlunoNaoExiste_DeveLancarNotFoundException()
        {
            // Arrange
            var dto = new CriarMatriculaDto { AlunoId = 99, TurmaId = 1 };
            _mockAlunoRepository.Setup(r => r.ObterPorIdAsync(99)).ReturnsAsync((Aluno)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(() => _service.RealizarMatriculaAsync(dto));
            Assert.That(ex.Message, Does.Contain("Aluno com ID 99 não foi encontrado."));
        }

        [Test]
        public void RealizarMatricula_QuandoAlunoInativo_DeveLancarBusinessException()
        {
            // Arrange
            var alunoInativo = new Aluno { Id = 4, Nome = "Diego Ferreira", Ativo = false };
            var dto = new CriarMatriculaDto { AlunoId = 4, TurmaId = 1 };
            _mockAlunoRepository.Setup(r => r.ObterPorIdAsync(4)).ReturnsAsync(alunoInativo);

            // Act & Assert
            var ex = Assert.ThrowsAsync<BusinessException>(() => _service.RealizarMatriculaAsync(dto));
            Assert.That(ex.Message, Does.Contain("está inativo e não pode realizar matrículas."));
            _mockTransaction.Verify(t => t.Commit(), Times.Never());
        }

        [Test]
        public void RealizarMatricula_QuandoTurmaNaoExiste_DeveLancarNotFoundExceptionERealizarRollback()
        {
            // Arrange
            var alunoAtivo = new Aluno { Id = 1, Nome = "Ana Souza", Ativo = true };
            var dto = new CriarMatriculaDto { AlunoId = 1, TurmaId = 99 };
            _mockAlunoRepository.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(alunoAtivo);
            _mockTurmaRepository.Setup(r => r.ObterPorIdAsync(99, It.IsAny<IDbTransaction>())).ReturnsAsync((Turma)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(() => _service.RealizarMatriculaAsync(dto));
            Assert.That(ex.Message, Does.Contain("Turma com ID 99 não foi encontrada."));
            _mockTransaction.Verify(t => t.Rollback(), Times.Once());
            _mockTransaction.Verify(t => t.Commit(), Times.Never());
        }

        [Test]
        public void RealizarMatricula_QuandoTurmaSemVagas_DeveLancarBusinessExceptionERealizarRollback()
        {
            // Arrange
            var alunoAtivo = new Aluno { Id = 1, Nome = "Ana Souza", Ativo = true };
            var turmaSemVagas = new Turma { Id = 4, Nome = "Turma Lotada", VagasDisponiveis = 0 };
            var dto = new CriarMatriculaDto { AlunoId = 1, TurmaId = 4 };

            _mockAlunoRepository.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(alunoAtivo);
            _mockTurmaRepository.Setup(r => r.ObterPorIdAsync(4, It.IsAny<IDbTransaction>())).ReturnsAsync(turmaSemVagas);

            // Act & Assert
            var ex = Assert.ThrowsAsync<BusinessException>(() => _service.RealizarMatriculaAsync(dto));
            Assert.That(ex.Message, Does.Contain("não possui vagas disponíveis."));
            _mockTransaction.Verify(t => t.Rollback(), Times.Once());
            _mockTransaction.Verify(t => t.Commit(), Times.Never());
        }

        [Test]
        public void RealizarMatricula_QuandoAlunoJaMatriculado_DeveLancarBusinessExceptionERealizarRollback()
        {
            // Arrange
            var alunoAtivo = new Aluno { Id = 1, Nome = "Ana Souza", Ativo = true };
            var turmaComVagas = new Turma { Id = 1, Nome = "3A - Ensino Medio", VagasDisponiveis = 5 };
            var dto = new CriarMatriculaDto { AlunoId = 1, TurmaId = 1 };

            _mockAlunoRepository.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(alunoAtivo);
            _mockTurmaRepository.Setup(r => r.ObterPorIdAsync(1, It.IsAny<IDbTransaction>())).ReturnsAsync(turmaComVagas);
            _mockMatriculaRepository.Setup(r => r.ExisteMatriculaAsync(1, 1, It.IsAny<IDbTransaction>())).ReturnsAsync(true);

            // Act & Assert
            var ex = Assert.ThrowsAsync<BusinessException>(() => _service.RealizarMatriculaAsync(dto));
            Assert.That(ex.Message, Does.Contain("já está matriculado na turma"));
            _mockTransaction.Verify(t => t.Rollback(), Times.Once());
            _mockTransaction.Verify(t => t.Commit(), Times.Never());
        }

        [Test]
        public void RealizarMatricula_QuandoDecrementoDeVagasFalhar_DeveLancarBusinessExceptionERealizarRollback()
        {
            // Arrange
            var alunoAtivo = new Aluno { Id = 1, Nome = "Ana Souza", Ativo = true };
            var turmaComVagas = new Turma { Id = 1, Nome = "3A - Ensino Medio", VagasDisponiveis = 1 };
            var dto = new CriarMatriculaDto { AlunoId = 1, TurmaId = 1 };

            _mockAlunoRepository.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(alunoAtivo);
            _mockTurmaRepository.Setup(r => r.ObterPorIdAsync(1, It.IsAny<IDbTransaction>())).ReturnsAsync(turmaComVagas);
            _mockMatriculaRepository.Setup(r => r.ExisteMatriculaAsync(1, 1, It.IsAny<IDbTransaction>())).ReturnsAsync(false);
            _mockMatriculaRepository.Setup(r => r.InserirAsync(It.IsAny<Matricula>(), It.IsAny<IDbTransaction>())).ReturnsAsync(10);
            
            // Simula falha de concorrência no decremento (outro processo pegou a última vaga)
            _mockTurmaRepository.Setup(r => r.DecrementarVagasAsync(1, It.IsAny<IDbTransaction>())).ReturnsAsync(false);

            // Act & Assert
            var ex = Assert.ThrowsAsync<BusinessException>(() => _service.RealizarMatriculaAsync(dto));
            Assert.That(ex.Message, Does.Contain("esgotaram-se durante o processamento."));
            _mockTransaction.Verify(t => t.Rollback(), Times.Once());
            _mockTransaction.Verify(t => t.Commit(), Times.Never());
        }

        [Test]
        public async Task RealizarMatricula_QuandoDadosValidos_DeveComitarTransacaoEInvalidarCache()
        {
            // Arrange
            var alunoAtivo = new Aluno { Id = 1, Nome = "Ana Souza", Ativo = true };
            var turmaComVagas = new Turma { Id = 2, Nome = "3B - Ensino Medio", VagasDisponiveis = 30 };
            var dto = new CriarMatriculaDto { AlunoId = 1, TurmaId = 2 };

            _mockAlunoRepository.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(alunoAtivo);
            _mockTurmaRepository.Setup(r => r.ObterPorIdAsync(2, It.IsAny<IDbTransaction>())).ReturnsAsync(turmaComVagas);
            _mockMatriculaRepository.Setup(r => r.ExisteMatriculaAsync(1, 2, It.IsAny<IDbTransaction>())).ReturnsAsync(false);
            _mockMatriculaRepository.Setup(r => r.InserirAsync(It.IsAny<Matricula>(), It.IsAny<IDbTransaction>())).ReturnsAsync(15);
            _mockTurmaRepository.Setup(r => r.DecrementarVagasAsync(2, It.IsAny<IDbTransaction>())).ReturnsAsync(true);

            // Act
            var resultado = await _service.RealizarMatriculaAsync(dto);

            // Assert
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Id, Is.EqualTo(15));
            Assert.That(resultado.AlunoId, Is.EqualTo(1));
            Assert.That(resultado.TurmaId, Is.EqualTo(2));
            Assert.That(resultado.NomeAluno, Is.EqualTo("Ana Souza"));
            Assert.That(resultado.NomeTurma, Is.EqualTo("3B - Ensino Medio"));

            // Validações da Transação ACID e Cache
            _mockTransaction.Verify(t => t.Commit(), Times.Once());
            _mockTransaction.Verify(t => t.Rollback(), Times.Never());
            _mockCacheService.Verify(c => c.InvalidarAsync(), Times.Once());
        }
    }
}
