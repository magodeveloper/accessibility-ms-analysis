using Analysis.Domain.Entities;
using Analysis.Application.Dtos;
using Analysis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Analysis.Application.Services
{
    public class ErrorService : IErrorService
    {
        private readonly AnalysisDbContext _context;
        public ErrorService(AnalysisDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorDto?> GetByIdAsync(int id)
        {
            var entity = await _context.Errors.FindAsync(id);
            return entity == null ? null : ToDto(entity);
        }

        public async Task<IEnumerable<ErrorDto>> GetByResultIdAsync(int resultId)
        {
            return await _context.Errors
                .Where(e => e.ResultId == resultId)
                .Select(e => ToDto(e))
                .ToListAsync();
        }

        public async Task<IEnumerable<ErrorDto>> GetAllAsync()
        {
            return await _context.Errors
                .Select(e => ToDto(e))
                .ToListAsync();
        }

        public async Task<ErrorDto> CreateAsync(ErrorDto dto)
        {
            var entity = new Error
            {
                ResultId = dto.ResultId,
                WcagCriterionId = dto.WcagCriterionId,
                ErrorCode = dto.ErrorCode,
                Description = dto.Description,
                Location = dto.Location,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Errors.Add(entity);
            await _context.SaveChangesAsync();
            return ToDto(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Errors.FindAsync(id);
            if (entity != null)
            {
                _context.Errors.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        private static ErrorDto ToDto(Error e) => new ErrorDto
        {
            Id = e.Id,
            ResultId = e.ResultId,
            WcagCriterionId = e.WcagCriterionId,
            ErrorCode = e.ErrorCode,
            Description = e.Description,
            Location = e.Location,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };
    }
}