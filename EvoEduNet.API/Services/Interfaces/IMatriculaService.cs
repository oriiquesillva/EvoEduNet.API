using System.Threading.Tasks;
using EvoEduNet.API.Domain.Dtos;

namespace EvoEduNet.API.Services.Interfaces
{
    public interface IMatriculaService
    {
        Task<MatriculaResponseDto> RealizarMatriculaAsync(CriarMatriculaDto dto);
    }
}
