namespace Analysis.Application.Dtos;

/// <summary>
/// DTO para error completo con sus detalles.
/// </summary>
public record CompleteErrorDto(
    int Id,
    int ResultId,
    int WcagCriterionId,
    string ErrorCode,
    string Description,
    string Location,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

/// <summary>
/// DTO para resultado completo con sus errores asociados.
/// </summary>
public record CompleteResultDto(
    int Id,
    int AnalysisId,
    int WcagCriterionId,
    string WcagCriterion,
    string Level,
    string Severity,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IEnumerable<CompleteErrorDto> Errors
);

/// <summary>
/// DTO para análisis completo con sus resultados y errores asociados.
/// </summary>
public record CompleteAnalysisDto(
    int Id,
    int UserId,
    DateTime DateAnalysis,
    string ContentType,
    string? ContentInput,
    string? SourceUrl,
    string ToolUsed,
    string Status,
    string SummaryResult,
    string ResultJson,
    int? DurationMs,
    string WcagVersion,
    string WcagLevel,
    int? AxeViolations,
    int? AxeNeedsReview,
    int? AxeRecommendations,
    int? AxePasses,
    int? AxeIncomplete,
    int? AxeInapplicable,
    int? EaViolations,
    int? EaNeedsReview,
    int? EaRecommendations,
    int? EaPasses,
    int? EaIncomplete,
    int? EaInapplicable,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IEnumerable<CompleteResultDto> Results
);
