using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EvoEduNet.API.Domain.Entities;

namespace EvoEduNet.API.Repositories.Interfaces
{
    public interface IAlunoRepository
    {
        Task<Tuple<IEnumerable<Aluno>, int>> ObterPaginadoAsync(string nome, int page, int pageSize);
        Task<IEnumerable<Aluno>> ObterTodosAsync(bool apenasAtivos = true);
        Task<Aluno> ObterPorIdAsync(int id);
        Task<Aluno> ObterPorEmailAsync(string email);
        Task<int> InserirAsync(Aluno aluno);
        Task<bool> AtualizarAsync(Aluno aluno);
        Task<bool> ExcluirLogicoAsync(int id);
    }
}
