using System.Collections.Generic;
using System.Threading.Tasks;
using EvoEduNet.API.Domain.Dtos;

namespace EvoEduNet.API.Services.Interfaces
{
    public interface IAlunoService
    {
        Task<PagedResultDto<AlunoResponseDto>> ObterPaginadoAsync(string nome, int page, int pageSize);
        Task<IEnumerable<AlunoResponseDto>> ObterTodosAsync(bool apenasAtivos = true);
        Task<AlunoResponseDto> ObterPorIdAsync(int id);
        Task<AlunoResponseDto> CriarAsync(CriarAlunoDto dto);
        Task<AlunoResponseDto> AtualizarAsync(int id, AtualizarAlunoDto dto);
        Task ExcluirLogicoAsync(int id);
    }
}
