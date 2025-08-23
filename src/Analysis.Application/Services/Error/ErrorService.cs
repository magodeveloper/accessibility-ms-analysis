using Analysis.Domain.Entities;
using Analysis.Application.Dtos;
using Analysis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ErrorEntity = Analysis.Domain.Entities.Error;

namespace Analysis.Application.Services.Error
{
    public class ErrorService : IErrorService
    {
        private readonly AnalysisDbContext _db;
        public ErrorService(AnalysisDbContext db)
        {
            _db = db;
        }

        public async Task<ErrorDto?> GetByIdAsync(int id)
        {
            var entity = await _db.Errors.FindAsync(id);
            return entity == null ? null : ToReadDto(entity);
        }

        public async Task<IEnumerable<ErrorDto>> GetByResultIdAsync(int resultId)
        {
            return await _db.Errors
                .Where(e => e.ResultId == resultId)
                .Select(e => ToReadDto(e))
                .ToListAsync();
        }

        public async Task<IEnumerable<ErrorDto>> GetAllAsync()
        {
            return await _db.Errors
                .Select(e => ToReadDto(e))
                .ToListAsync();
        }

        public async Task<ErrorDto> CreateAsync(ErrorCreateDto dto)
        {
            var entity = new ErrorEntity
            {
                ResultId = dto.ResultId,
                WcagCriterionId = dto.WcagCriterionId,
                ErrorCode = dto.ErrorCode,
                Description = dto.Description,
                Location = dto.Location,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Result = null!
            };
            _db.Errors.Add(entity);
            await _db.SaveChangesAsync();
            return ToReadDto(entity);
        }


        public async Task DeleteAsync(int id)
        {
            var entity = await _db.Errors.FindAsync(id);
            if (entity != null)
            {
                _db.Errors.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteAllAsync()
        {
            var allEntities = await _db.Errors.ToListAsync();
            _db.Errors.RemoveRange(allEntities);
            await _db.SaveChangesAsync();
        }

        private static ErrorDto ToReadDto(ErrorEntity e) => new ErrorDto(
            e.Id,
            e.ResultId,
            e.WcagCriterionId,
            e.ErrorCode,
            e.Description,
            e.Location,
            string.Empty, // Message no existe en entidad
            string.Empty, // Code no existe en entidad
            e.CreatedAt,
            e.UpdatedAt
        );
    }
}