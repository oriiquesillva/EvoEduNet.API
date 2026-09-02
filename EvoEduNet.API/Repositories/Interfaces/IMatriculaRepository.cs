using System.Data;
using System.Threading.Tasks;
using EvoEduNet.API.Domain.Entities;

namespace EvoEduNet.API.Repositories.Interfaces
{
    public interface IMatriculaRepository
    {
        Task<int> InserirAsync(Matricula matricula, IDbTransaction transaction);
        Task<bool> ExisteMatriculaAsync(int alunoId, int turmaId, IDbTransaction transaction = null);
    }
}
