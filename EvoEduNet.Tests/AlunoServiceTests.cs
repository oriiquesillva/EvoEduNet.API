using System;
using System.Threading.Tasks;
using EvoEduNet.API.Domain.Dtos;
using EvoEduNet.API.Domain.Entities;
using EvoEduNet.API.Domain.Exceptions;
using EvoEduNet.API.Repositories.Interfaces;
using EvoEduNet.API.Services;
using Moq;
using NUnit.Framework;

namespace EvoEduNet.Tests
{
    [TestFixture]
    public class AlunoServiceTests
    {
        private Mock<IAlunoRepository> _mockAlunoRepository;
        private AlunoService _service;

        [SetUp]
        public void Setup()
        {
            _mockAlunoRepository = new Mock<IAlunoRepository>();
            _service = new AlunoService(_mockAlunoRepository.Object);
        }

        [Test]
        public void Criar_QuandoEmailJaCadastrado_DeveLancarBusinessException()
        {
            // Arrange
            var dto = new CriarAlunoDto
            {
                Nome = "Novo Aluno",
                Email = "existente@email.com",
                DataNascimento = new DateTime(2005, 1, 1)
            };
            var alunoExistente = new Aluno { Id = 1, Nome = "Outro Aluno", Email = "existente@email.com" };
            _mockAlunoRepository.Setup(r => r.ObterPorEmailAsync("existente@email.com")).ReturnsAsync(alunoExistente);

            // Act & Assert
            var ex = Assert.ThrowsAsync<BusinessException>(() => _service.CriarAsync(dto));
            Assert.That(ex.Message, Does.Contain("Já existe um aluno cadastrado com o e-mail"));
            _mockAlunoRepository.Verify(r => r.InserirAsync(It.IsAny<Aluno>()), Times.Never());
        }

        [Test]
        public async Task ExcluirLogico_QuandoAlunoExiste_DeveChamarExcluirLogicoNoRepositorio()
        {
            // Arrange
            var alunoAtivo = new Aluno { Id = 5, Nome = "Aluno Teste", Ativo = true };
            _mockAlunoRepository.Setup(r => r.ObterPorIdAsync(5)).ReturnsAsync(alunoAtivo);
            _mockAlunoRepository.Setup(r => r.ExcluirLogicoAsync(5)).ReturnsAsync(true);

            // Act
            await _service.ExcluirLogicoAsync(5);

            // Assert
            _mockAlunoRepository.Verify(r => r.ExcluirLogicoAsync(5), Times.Once());
        }

        [Test]
        public void ExcluirLogico_QuandoAlunoNaoExiste_DeveLancarNotFoundException()
        {
            // Arrange
            _mockAlunoRepository.Setup(r => r.ObterPorIdAsync(999)).ReturnsAsync((Aluno)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(() => _service.ExcluirLogicoAsync(999));
            Assert.That(ex.Message, Does.Contain("Aluno com ID 999 não foi encontrado."));
        }

        [Test]
        public async Task ObterTodos_QuandoApenasAtivosTrue_DeveChamarRepositorioComTrueERetornarLista()
        {
            // Arrange
            var alunos = new System.Collections.Generic.List<Aluno>
            {
                new Aluno { Id = 1, Nome = "Ana Souza", Email = "ana@email.com", Ativo = true }
            };
            _mockAlunoRepository.Setup(r => r.ObterTodosAsync(true)).ReturnsAsync(alunos);

            // Act
            var resultado = await _service.ObterTodosAsync(true);

            // Assert
            Assert.That(System.Linq.Enumerable.Count(resultado), Is.EqualTo(1));
            Assert.That(System.Linq.Enumerable.First(resultado).Nome, Is.EqualTo("Ana Souza"));
            _mockAlunoRepository.Verify(r => r.ObterTodosAsync(true), Times.Once());
        }

        [Test]
        public async Task ObterTodos_QuandoApenasAtivosFalse_DeveChamarRepositorioComFalseERetornarLista()
        {
            // Arrange
            var alunos = new System.Collections.Generic.List<Aluno>
            {
                new Aluno { Id = 1, Nome = "Ana Souza", Email = "ana@email.com", Ativo = true },
                new Aluno { Id = 4, Nome = "Diego Ferreira", Email = "diego@email.com", Ativo = false }
            };
            _mockAlunoRepository.Setup(r => r.ObterTodosAsync(false)).ReturnsAsync(alunos);

            // Act
            var resultado = await _service.ObterTodosAsync(false);

            // Assert
            Assert.That(System.Linq.Enumerable.Count(resultado), Is.EqualTo(2));
            _mockAlunoRepository.Verify(r => r.ObterTodosAsync(false), Times.Once());
        }
    }
}
