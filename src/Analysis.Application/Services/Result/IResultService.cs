using System.Threading.Tasks;
using Analysis.Application.Dtos;
using System.Collections.Generic;

namespace Analysis.Application.Services.Result
{
    public interface IResultService
    {
        Task<ResultDto?> GetByIdAsync(int id);
        Task<IEnumerable<ResultDto>> GetByAnalysisIdAsync(int analysisId);
        Task<IEnumerable<ResultDto>> GetAllAsync();
        Task<ResultDto> CreateAsync(ResultCreateDto dto);
        Task DeleteAsync(int id);
    }
}