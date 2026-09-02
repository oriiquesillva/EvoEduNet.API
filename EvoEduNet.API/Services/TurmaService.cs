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
    public class TurmaService : ITurmaService
    {
        private readonly ITurmaRepository _turmaRepository;
        private readonly ITurmaCacheService _cacheService;

        public TurmaService(ITurmaRepository turmaRepository, ITurmaCacheService cacheService)
        {
            _turmaRepository = turmaRepository ?? throw new ArgumentNullException(nameof(turmaRepository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        public async Task<IEnumerable<TurmaResponseDto>> ObterTodasAsync()
        {
            // 1. Tentar obter do cache primeiro
            var turmasEmCache = await _cacheService.ObterTurmasAsync();
            if (turmasEmCache != null)
            {
                return turmasEmCache;
            }

            // 2. Se não estiver em cache, consultar no banco
            var turmas = await _turmaRepository.ObterTodasAsync();
            var dtos = turmas.Select(MapToResponseDto).ToList();

            // 3. Salvar no cache
            await _cacheService.InserirAsync(dtos);

            return dtos;
        }

        public async Task<TurmaResponseDto> ObterPorIdAsync(int id)
        {
            var turma = await _turmaRepository.ObterPorIdAsync(id);
            if (turma == null)
            {
                throw new NotFoundException($"Turma com ID {id} não foi encontrada.");
            }

            return MapToResponseDto(turma);
        }

        private static TurmaResponseDto MapToResponseDto(Turma turma)
        {
            return new TurmaResponseDto
            {
                Id = turma.Id,
                Nome = turma.Nome,
                Periodo = turma.Periodo,
                VagasTotal = turma.VagasTotal,
                VagasDisponiveis = turma.VagasDisponiveis
            };
        }
    }
}
