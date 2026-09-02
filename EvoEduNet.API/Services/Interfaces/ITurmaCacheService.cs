using System.Collections.Generic;
using System.Threading.Tasks;
using EvoEduNet.API.Domain.Dtos;

namespace EvoEduNet.API.Services.Interfaces
{
    public interface ITurmaCacheService
    {
        Task<IEnumerable<TurmaResponseDto>> ObterTurmasAsync();
        Task InserirAsync(IEnumerable<TurmaResponseDto> turmas);
        Task InvalidarAsync();
    }
}
