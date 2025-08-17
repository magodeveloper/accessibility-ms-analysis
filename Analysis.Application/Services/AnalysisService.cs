using Analysis.Infrastructure;
using Analysis.Domain.Entities;
using Analysis.Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Analysis.Application.Services
{
    public class AnalysisService : IAnalysisService
    {
        private readonly AnalysisDbContext _context;
        public AnalysisService(AnalysisDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AnalysisDto>> GetByDateAsync(int userId, DateTime date)
        {
            return await _context.Analyses
                .Where(a => a.UserId == userId && a.DateAnalysis.Date == date.Date)
                .Select(a => ToDto(a))
                .ToListAsync();
        }

        public async Task<IEnumerable<AnalysisDto>> GetByToolAsync(int userId, string toolUsed)
        {
            return await _context.Analyses
                .Where(a => a.UserId == userId && a.ToolUsed == toolUsed)
                .Select(a => ToDto(a))
                .ToListAsync();
        }

        public async Task<IEnumerable<AnalysisDto>> GetByStatusAsync(int userId, string status)
        {
            return await _context.Analyses
                .Where(a => a.UserId == userId && a.Status == status)
                .Select(a => ToDto(a))
                .ToListAsync();
        }

        public async Task<AnalysisDto?> GetByIdAsync(int id)
        {
            var entity = await _context.Analyses.FindAsync(id);
            return entity == null ? null : ToDto(entity);
        }

        public async Task<AnalysisDto> CreateAsync(AnalysisDto dto)
        {
            var entity = new Analysis.Domain.Entities.Analysis
            {
                UserId = dto.UserId,
                DateAnalysis = dto.DateAnalysis,
                ContentType = dto.ContentType,
                ContentInput = dto.ContentInput,
                SourceUrl = dto.SourceUrl,
                ToolUsed = dto.ToolUsed,
                Status = dto.Status,
                SummaryResult = dto.SummaryResult,
                ResultJson = dto.ResultJson,
                ErrorMessage = dto.ErrorMessage,
                DurationMs = dto.DurationMs,
                WcagVersion = dto.WcagVersion,
                WcagLevel = dto.WcagLevel,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Analyses.Add(entity);
            await _context.SaveChangesAsync();
            return ToDto(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Analyses.FindAsync(id);
            if (entity != null)
            {
                _context.Analyses.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        private static AnalysisDto ToDto(Analysis.Domain.Entities.Analysis a) => new AnalysisDto
        {
            Id = a.Id,
            UserId = a.UserId,
            DateAnalysis = a.DateAnalysis,
            ContentType = a.ContentType,
            ContentInput = a.ContentInput,
            SourceUrl = a.SourceUrl,
            ToolUsed = a.ToolUsed,
            Status = a.Status,
            SummaryResult = a.SummaryResult,
            ResultJson = a.ResultJson,
            ErrorMessage = a.ErrorMessage,
            DurationMs = a.DurationMs,
            WcagVersion = a.WcagVersion,
            WcagLevel = a.WcagLevel,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        };
    }
}