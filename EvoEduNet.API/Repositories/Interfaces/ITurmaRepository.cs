using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using EvoEduNet.API.Domain.Entities;

namespace EvoEduNet.API.Repositories.Interfaces
{
    public interface ITurmaRepository
    {
        Task<IEnumerable<Turma>> ObterTodasAsync();
        Task<Turma> ObterPorIdAsync(int id, IDbTransaction transaction = null);
        Task<bool> DecrementarVagasAsync(int turmaId, IDbTransaction transaction);
    }
}
