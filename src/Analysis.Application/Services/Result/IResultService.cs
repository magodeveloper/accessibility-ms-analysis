using Analysis.Application.Dtos;

namespace Analysis.Application.Services.Result;

public interface IResultService
{
    Task<ResultDto?> GetByIdAsync(int id);
    Task<IEnumerable<ResultDto>> GetByAnalysisIdAsync(int analysisId);
    Task<IEnumerable<ResultDto>> GetAllAsync();
    Task<IEnumerable<ResultDto>> GetByLevelAsync(string level);
    Task<IEnumerable<ResultDto>> GetBySeverityAsync(string severity);
    Task<ResultDto> CreateAsync(ResultCreateDto dto);
    Task DeleteAsync(int id);
    Task DeleteAllAsync();
}
