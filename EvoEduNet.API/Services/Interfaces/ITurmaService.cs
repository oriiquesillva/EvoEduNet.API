using System.Collections.Generic;
using System.Threading.Tasks;
using EvoEduNet.API.Domain.Dtos;

namespace EvoEduNet.API.Services.Interfaces
{
    public interface ITurmaService
    {
        Task<IEnumerable<TurmaResponseDto>> ObterTodasAsync();
        Task<TurmaResponseDto> ObterPorIdAsync(int id);
    }
}
