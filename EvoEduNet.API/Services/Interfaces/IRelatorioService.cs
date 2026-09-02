using System.Collections.Generic;
using System.Threading.Tasks;
using EvoEduNet.API.Domain.Dtos;

namespace EvoEduNet.API.Services.Interfaces
{
    public interface IRelatorioService
    {
        Task<IEnumerable<RelatorioAlunoTurmaDto>> ObterAlunosPorTurmaAsync();
    }
}
