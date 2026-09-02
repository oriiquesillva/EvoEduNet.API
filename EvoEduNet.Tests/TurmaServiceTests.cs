using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EvoEduNet.API.Domain.Dtos;
using EvoEduNet.API.Domain.Entities;
using EvoEduNet.API.Domain.Exceptions;
using EvoEduNet.API.Repositories.Interfaces;
using EvoEduNet.API.Services;
using EvoEduNet.API.Services.Interfaces;
using Moq;
using NUnit.Framework;

namespace EvoEduNet.Tests
{
    [TestFixture]
    public class TurmaServiceTests
    {
        private Mock<ITurmaRepository> _mockTurmaRepository;
        private Mock<ITurmaCacheService> _mockCacheService;
        private TurmaService _service;

        [SetUp]
        public void Setup()
        {
            _mockTurmaRepository = new Mock<ITurmaRepository>();
            _mockCacheService = new Mock<ITurmaCacheService>();
            _service = new TurmaService(_mockTurmaRepository.Object, _mockCacheService.Object);
        }

        [Test]
        public async Task ObterTodas_QuandoExisteNoCache_DeveRetornarDoCacheSemConsultarBanco()
        {
            // Arrange (Cache HIT)
            var turmasCache = new List<TurmaResponseDto>
            {
                new TurmaResponseDto { Id = 1, Nome = "Turma Cache", VagasDisponiveis = 10, VagasTotal = 20 }
            };
            _mockCacheService.Setup(c => c.ObterTurmasAsync()).ReturnsAsync(turmasCache);

            // Act
            var resultado = await _service.ObterTodasAsync();

            // Assert
            Assert.That(resultado.Count(), Is.EqualTo(1));
            Assert.That(resultado.First().Nome, Is.EqualTo("Turma Cache"));
            _mockTurmaRepository.Verify(r => r.ObterTodasAsync(), Times.Never());
        }

        [Test]
        public async Task ObterTodas_QuandoNaoExisteNoCache_DeveConsultarBancoEPopularCache()
        {
            // Arrange (Cache MISS)
            _mockCacheService.Setup(c => c.ObterTurmasAsync()).ReturnsAsync((IEnumerable<TurmaResponseDto>)null);
            var turmasBanco = new List<Turma>
            {
                new Turma { Id = 1, Nome = "3A - Ensino Medio", Periodo = "Manha", VagasTotal = 30, VagasDisponiveis = 28 }
            };
            _mockTurmaRepository.Setup(r => r.ObterTodasAsync()).ReturnsAsync(turmasBanco);

            // Act
            var resultado = await _service.ObterTodasAsync();

            // Assert
            Assert.That(resultado.Count(), Is.EqualTo(1));
            Assert.That(resultado.First().Nome, Is.EqualTo("3A - Ensino Medio"));
            _mockTurmaRepository.Verify(r => r.ObterTodasAsync(), Times.Once());
            _mockCacheService.Verify(c => c.InserirAsync(It.IsAny<IEnumerable<TurmaResponseDto>>()), Times.Once());
        }

        [Test]
        public void ObterPorId_QuandoTurmaNaoExiste_DeveLancarNotFoundException()
        {
            // Arrange
            _mockTurmaRepository.Setup(r => r.ObterPorIdAsync(999, null)).ReturnsAsync((Turma)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(() => _service.ObterPorIdAsync(999));
            Assert.That(ex.Message, Does.Contain("Turma com ID 999 não foi encontrada."));
        }
    }
}
