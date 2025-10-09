using Analysis.Application.Dtos;

namespace Analysis.Application.Services.Error;

public interface IErrorService
{
    Task<ErrorDto?> GetByIdAsync(int id);
    Task<IEnumerable<ErrorDto>> GetByResultIdAsync(int resultId);
    Task<IEnumerable<ErrorDto>> GetAllAsync();
    Task<ErrorDto> CreateAsync(ErrorCreateDto dto);
    Task DeleteAsync(int id);
    Task DeleteAllAsync();
}
