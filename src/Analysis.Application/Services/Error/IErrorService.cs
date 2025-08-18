using System.Threading.Tasks;
using Analysis.Application.Dtos;
using System.Collections.Generic;

namespace Analysis.Application.Services.Error
{
    public interface IErrorService
    {
        Task<ErrorDto?> GetByIdAsync(int id);
        Task<IEnumerable<ErrorDto>> GetByResultIdAsync(int resultId);
        Task<IEnumerable<ErrorDto>> GetAllAsync();
        Task<ErrorDto> CreateAsync(ErrorCreateDto dto);
        Task DeleteAsync(int id);
    }
}