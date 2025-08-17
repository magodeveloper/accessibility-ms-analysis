using Analysis.Infrastructure;
using Analysis.Domain.Entities;
using Analysis.Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Analysis.Application.Services
{
    public class ResultService : IResultService
    {
        private readonly AnalysisDbContext _context;
        public ResultService(AnalysisDbContext context)
        {
            _context = context;
        }

        public async Task<ResultDto?> GetByIdAsync(int id)
        {
            var entity = await _context.Results.FindAsync(id);
            return entity == null ? null : ToDto(entity);
        }

        public async Task<IEnumerable<ResultDto>> GetByAnalysisIdAsync(int analysisId)
        {
            return await _context.Results
                .Where(r => r.AnalysisId == analysisId)
                .Select(r => ToDto(r))
                .ToListAsync();
        }

        public async Task<IEnumerable<ResultDto>> GetAllAsync()
        {
            return await _context.Results
                .Select(r => ToDto(r))
                .ToListAsync();
        }

        public async Task<ResultDto> CreateAsync(ResultDto dto)
        {
            var entity = new Result
            {
                AnalysisId = dto.AnalysisId,
                WcagCriterionId = dto.WcagCriterionId,
                WcagCriterion = dto.WcagCriterion,
                Level = dto.Level,
                Severity = dto.Severity,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Results.Add(entity);
            await _context.SaveChangesAsync();
            return ToDto(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Results.FindAsync(id);
            if (entity != null)
            {
                _context.Results.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        private static ResultDto ToDto(Result r) => new ResultDto
        {
            Id = r.Id,
            AnalysisId = r.AnalysisId,
            WcagCriterionId = r.WcagCriterionId,
            WcagCriterion = r.WcagCriterion,
            Level = r.Level,
            Severity = r.Severity,
            Description = r.Description,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }
}