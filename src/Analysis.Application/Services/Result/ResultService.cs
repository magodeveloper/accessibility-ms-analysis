using ErrorEntity = Analysis.Domain.Entities.Error;
using Analysis.Domain.Entities;
using Analysis.Application.Dtos;
using Analysis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ResultEntity = Analysis.Domain.Entities.Result;

namespace Analysis.Application.Services.Result
{
    public class ResultService : IResultService
    {
        private readonly AnalysisDbContext _db;
        public ResultService(AnalysisDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<ResultDto>> GetByLevelAsync(string level)
        {
            return await _db.Results
                .Where(r => r.Level.ToString().ToLower() == level.ToLower())
                .Select(r => ToReadDto(r))
                .ToListAsync();
        }

        public async Task<IEnumerable<ResultDto>> GetBySeverityAsync(string severity)
        {
            return await _db.Results
                .Where(r => r.Severity.ToString().ToLower() == severity.ToLower())
                .Select(r => ToReadDto(r))
                .ToListAsync();
        }

        public async Task<ResultDto?> GetByIdAsync(int id)
        {
            var entity = await _db.Results.FindAsync(id);
            return entity == null ? null : ToReadDto(entity);
        }

        public async Task<IEnumerable<ResultDto>> GetByAnalysisIdAsync(int analysisId)
        {
            return await _db.Results
                .Where(r => r.AnalysisId == analysisId)
                .Select(r => ToReadDto(r))
                .ToListAsync();
        }

        public async Task<IEnumerable<ResultDto>> GetAllAsync()
        {
            return await _db.Results
                .Select(r => ToReadDto(r))
                .ToListAsync();
        }

        public async Task<ResultDto> CreateAsync(ResultCreateDto dto)
        {
            var entity = new ResultEntity
            {
                AnalysisId = dto.AnalysisId,
                WcagCriterionId = dto.WcagCriterionId,
                WcagCriterion = dto.WcagCriterion,
                Level = Enum.TryParse<ResultLevel>(dto.Level, true, out var lvl) ? lvl : ResultLevel.violation,
                Severity = Enum.TryParse<Severity>(dto.Severity, true, out var sev) ? sev : Severity.medium,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Analysis = null!,
                Errors = new List<ErrorEntity>()
            };
            _db.Results.Add(entity);
            await _db.SaveChangesAsync();
            return ToReadDto(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _db.Results.FindAsync(id);
            if (entity != null)
            {
                _db.Results.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteAllAsync()
        {
            var allEntities = await _db.Results.ToListAsync();
            _db.Results.RemoveRange(allEntities);
            await _db.SaveChangesAsync();

            // Reset AUTO_INCREMENT to 1
            await _db.Database.ExecuteSqlRawAsync("ALTER TABLE RESULTS AUTO_INCREMENT = 1");
        }

        private static ResultDto ToReadDto(ResultEntity r) => new ResultDto(
                    r.Id,
                    r.AnalysisId,
                    r.WcagCriterionId,
                    r.WcagCriterion,
                    r.Level.ToString(),
                    r.Severity.ToString(),
                    r.Description,
                    r.CreatedAt,
                    r.UpdatedAt
                );
    }
}