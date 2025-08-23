using System.Threading.Tasks;
using Analysis.Application.Dtos;
using System.Collections.Generic;

namespace Analysis.Application.Services.Analysis
{
    public interface IAnalysisService
    {
        Task<IEnumerable<AnalysisDto>> GetAllAsync();
        Task<IEnumerable<AnalysisDto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<AnalysisDto>> GetByDateAsync(int userId, DateTime date);
        Task<IEnumerable<AnalysisDto>> GetByToolAsync(int userId, string toolUsed);
        Task<IEnumerable<AnalysisDto>> GetByStatusAsync(int userId, string status);
        Task<AnalysisDto?> GetByIdAsync(int id);
        Task<AnalysisDto> CreateAsync(AnalysisCreateDto dto);
        Task DeleteAsync(int id);
    }
}