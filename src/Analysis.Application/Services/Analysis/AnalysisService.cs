using Analysis.Domain.Entities;
using Analysis.Domain.Services;
using Analysis.Application.Dtos;
using Analysis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ResultEntity = Analysis.Domain.Entities.Result;
using AnalysisEntity = Analysis.Domain.Entities.Analysis;

namespace Analysis.Application.Services.Analysis
{
    public class AnalysisService : IAnalysisService
    {
        private readonly AnalysisDbContext _db;
        private readonly IUserValidationService _userValidationService;
        private readonly ILogger<AnalysisService> _logger;

        public AnalysisService(
            AnalysisDbContext db,
            IUserValidationService userValidationService,
            ILogger<AnalysisService> logger)
        {
            _db = db;
            _userValidationService = userValidationService;
            _logger = logger;
        }

        public async Task<IEnumerable<AnalysisDto>> GetAllAsync()
        {
            return await _db.Analyses
                .Select(a => ToReadDto(a))
                .ToListAsync();
        }

        public async Task<IEnumerable<AnalysisDto>> GetByUserIdAsync(int userId)
        {
            _logger.LogDebug("Getting analyses for UserId: {UserId}", userId);

            // Validar que el usuario existe antes de buscar sus análisis
            var userIsValid = await _userValidationService.ValidateUserExistsAsync(userId);
            if (!userIsValid)
            {
                _logger.LogWarning("Attempt to get analyses for non-existent UserId: {UserId}", userId);
                // En lugar de lanzar excepción, retornar lista vacía para mejor UX
                return Enumerable.Empty<AnalysisDto>();
            }

            return await _db.Analyses
                .Where(a => a.UserId == userId)
                .Select(a => ToReadDto(a))
                .ToListAsync();
        }

        public async Task<IEnumerable<AnalysisDto>> GetByDateAsync(int userId, DateTime date)
        {
            return await _db.Analyses
                .Where(a => a.UserId == userId && a.DateAnalysis.Date == date.Date)
                .Select(a => ToReadDto(a))
                .ToListAsync();
        }

        public async Task<IEnumerable<AnalysisDto>> GetByToolAsync(int userId, string toolUsed)
        {
            return await _db.Analyses
                .Where(a => a.UserId == userId && a.ToolUsed.ToString() == toolUsed)
                .Select(a => ToReadDto(a))
                .ToListAsync();
        }

        public async Task<IEnumerable<AnalysisDto>> GetByStatusAsync(int userId, string status)
        {
            return await _db.Analyses
                .Where(a => a.UserId == userId && a.Status.ToString() == status)
                .Select(a => ToReadDto(a))
                .ToListAsync();
        }

        public async Task<AnalysisDto?> GetByIdAsync(int id)
        {
            var entity = await _db.Analyses.FindAsync(id);
            return entity == null ? null : ToReadDto(entity);
        }

        public async Task<AnalysisDto> CreateAsync(AnalysisCreateDto dto)
        {
            _logger.LogInformation("Creating analysis for UserId: {UserId}", dto.UserId);

            // Validar que el usuario existe antes de crear el análisis
            var userIsValid = await _userValidationService.ValidateUserExistsAsync(dto.UserId);
            if (!userIsValid)
            {
                _logger.LogWarning("Attempt to create analysis for non-existent UserId: {UserId}", dto.UserId);
                throw new ArgumentException($"User with ID {dto.UserId} does not exist or is not valid.", nameof(dto));
            }

            var entity = new AnalysisEntity
            {
                UserId = dto.UserId,
                DateAnalysis = dto.DateAnalysis,
                ContentType = Enum.TryParse<ContentType>(dto.ContentType, true, out var ct) ? ct : ContentType.url,
                ContentInput = dto.ContentInput,
                SourceUrl = dto.SourceUrl,
                ToolUsed = Enum.TryParse<ToolUsed>(dto.ToolUsed, true, out var tu) ? tu : ToolUsed.axecore,
                Status = Enum.TryParse<AnalysisStatus>(dto.Status, true, out var st) ? st : AnalysisStatus.pending,
                SummaryResult = dto.SummaryResult,
                ResultJson = dto.ResultJson,
                DurationMs = dto.DurationMs,
                WcagVersion = dto.WcagVersion,
                WcagLevel = Enum.TryParse<WcagLevel>(dto.WcagLevel, true, out var wl) ? wl : WcagLevel.A,
                AxeViolations = dto.AxeViolations,
                AxeNeedsReview = dto.AxeNeedsReview,
                AxeRecommendations = dto.AxeRecommendations,
                AxePasses = dto.AxePasses,
                AxeIncomplete = dto.AxeIncomplete,
                AxeInapplicable = dto.AxeInapplicable,
                EaViolations = dto.EaViolations,
                EaNeedsReview = dto.EaNeedsReview,
                EaRecommendations = dto.EaRecommendations,
                EaPasses = dto.EaPasses,
                EaIncomplete = dto.EaIncomplete,
                EaInapplicable = dto.EaInapplicable,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Results = new List<ResultEntity>()
            };

            _db.Analyses.Add(entity);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Analysis created successfully with ID: {AnalysisId} for UserId: {UserId}",
                entity.Id, dto.UserId);

            return ToReadDto(entity);
        }


        public async Task DeleteAsync(int id)
        {
            var entity = await _db.Analyses.FindAsync(id);
            if (entity != null)
            {
                _db.Analyses.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteAllAsync()
        {
            var allEntities = await _db.Analyses.ToListAsync();
            _db.Analyses.RemoveRange(allEntities);
            await _db.SaveChangesAsync();

            // Reset AUTO_INCREMENT to 1
            await _db.Database.ExecuteSqlRawAsync("ALTER TABLE ANALYSIS AUTO_INCREMENT = 1");
        }

        private static AnalysisDto ToReadDto(AnalysisEntity a) => new AnalysisDto(
            a.Id,
            a.UserId,
            a.DateAnalysis,
            a.ContentType.ToString(),
            a.ContentInput,
            a.SourceUrl,
            a.ToolUsed.ToString(),
            a.Status.ToString(),
            a.SummaryResult,
            a.ResultJson,
            a.DurationMs,
            a.WcagVersion,
            a.WcagLevel.ToString(),
            a.AxeViolations,
            a.AxeNeedsReview,
            a.AxeRecommendations,
            a.AxePasses,
            a.AxeIncomplete,
            a.AxeInapplicable,
            a.EaViolations,
            a.EaNeedsReview,
            a.EaRecommendations,
            a.EaPasses,
            a.EaIncomplete,
            a.EaInapplicable,
            a.CreatedAt,
            a.UpdatedAt
        );
    }
}