using System.Collections.Generic;
using System.Threading.Tasks;
using EvoEduNet.API.Domain.Dtos;

namespace EvoEduNet.API.Repositories.Interfaces
{
    public interface IRelatorioRepository
    {
        Task<IEnumerable<RelatorioAlunoTurmaDto>> ObterAlunosPorTurmaAsync();
    }
}
