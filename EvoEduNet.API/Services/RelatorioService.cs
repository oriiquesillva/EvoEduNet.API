using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EvoEduNet.API.Domain.Dtos;
using EvoEduNet.API.Repositories.Interfaces;
using EvoEduNet.API.Services.Interfaces;

namespace EvoEduNet.API.Services
{
    public class RelatorioService : IRelatorioService
    {
        private readonly IRelatorioRepository _relatorioRepository;

        public RelatorioService(IRelatorioRepository relatorioRepository)
        {
            _relatorioRepository = relatorioRepository ?? throw new ArgumentNullException(nameof(relatorioRepository));
        }

        public async Task<IEnumerable<RelatorioAlunoTurmaDto>> ObterAlunosPorTurmaAsync()
        {
            return await _relatorioRepository.ObterAlunosPorTurmaAsync();
        }
    }
}
