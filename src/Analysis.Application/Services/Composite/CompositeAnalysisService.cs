using Analysis.Application.Dtos;
using Analysis.Application.Services.Error;
using Analysis.Application.Services.Result;
using Analysis.Application.Services.Analysis;

namespace Analysis.Application.Services.Composite;

/// <summary>
/// Implementación del servicio compuesto para obtener análisis completos.
/// </summary>
public class CompositeAnalysisService : ICompositeAnalysisService
{
    private readonly IAnalysisService _analysisService;
    private readonly IResultService _resultService;
    private readonly IErrorService _errorService;

    public CompositeAnalysisService(
        IAnalysisService analysisService,
        IResultService resultService,
        IErrorService errorService)
    {
        _analysisService = analysisService;
        _resultService = resultService;
        _errorService = errorService;
    }

    /// <summary>
    /// Obtiene un análisis completo por ID incluyendo resultados y errores.
    /// </summary>
    public async Task<CompleteAnalysisDto?> GetCompleteAnalysisByIdAsync(int analysisId)
    {
        // 1. Obtener el análisis
        var analysis = await _analysisService.GetByIdAsync(analysisId);
        if (analysis == null)
        {
            return null;
        }

        // 2. Obtener los resultados del análisis
        var results = await _resultService.GetByAnalysisIdAsync(analysisId);

        // 3. Para cada resultado, obtener sus errores
        var completeResults = new List<CompleteResultDto>();
        foreach (var result in results)
        {
            var errors = await _errorService.GetByResultIdAsync(result.Id);

            var completeErrors = errors.Select(e => new CompleteErrorDto(
                e.Id,
                e.ResultId,
                e.WcagCriterionId,
                e.ErrorCode,
                e.Description,
                e.Location,
                e.CreatedAt,
                e.UpdatedAt
            ));

            completeResults.Add(new CompleteResultDto(
                result.Id,
                result.AnalysisId,
                result.WcagCriterionId,
                result.WcagCriterion,
                result.Level,
                result.Severity,
                result.Description,
                result.CreatedAt,
                result.UpdatedAt,
                completeErrors
            ));
        }

        // 4. Construir el análisis completo
        return new CompleteAnalysisDto(
            analysis.Id,
            analysis.UserId,
            analysis.DateAnalysis,
            analysis.ContentType,
            analysis.ContentInput,
            analysis.SourceUrl,
            analysis.ToolUsed,
            analysis.Status,
            analysis.SummaryResult,
            analysis.ResultJson,
            analysis.DurationMs,
            analysis.WcagVersion,
            analysis.WcagLevel,
            analysis.AxeViolations,
            analysis.AxeNeedsReview,
            analysis.AxeRecommendations,
            analysis.AxePasses,
            analysis.AxeIncomplete,
            analysis.AxeInapplicable,
            analysis.EaViolations,
            analysis.EaNeedsReview,
            analysis.EaRecommendations,
            analysis.EaPasses,
            analysis.EaIncomplete,
            analysis.EaInapplicable,
            analysis.CreatedAt,
            analysis.UpdatedAt,
            completeResults
        );
    }

    /// <summary>
    /// Obtiene todos los análisis completos de un usuario incluyendo resultados y errores.
    /// </summary>
    public async Task<IEnumerable<CompleteAnalysisDto>> GetCompleteAnalysesByUserIdAsync(int userId)
    {
        // 1. Obtener todos los análisis del usuario
        var analyses = await _analysisService.GetByUserIdAsync(userId);

        // 2. Para cada análisis, obtener su versión completa
        var completeAnalyses = new List<CompleteAnalysisDto>();
        foreach (var analysis in analyses)
        {
            var completeAnalysis = await GetCompleteAnalysisByIdAsync(analysis.Id);
            if (completeAnalysis != null)
            {
                completeAnalyses.Add(completeAnalysis);
            }
        }

        return completeAnalyses;
    }
}
