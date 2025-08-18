namespace Analysis.Application.Dtos;

public record AnalysisDto(
    int Id,
    int UserId,
    DateTime DateAnalysis,
    string ContentType,
    string ContentInput,
    string SourceUrl,
    string ToolUsed,
    string Status,
    string SummaryResult,
    string ResultJson,
    string ErrorMessage,
    int? DurationMs,
    string WcagVersion,
    string WcagLevel,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record AnalysisCreateDto(
    int UserId,
    DateTime DateAnalysis,
    string ContentType,
    string ContentInput,
    string SourceUrl,
    string ToolUsed,
    string Status,
    string SummaryResult,
    string ResultJson,
    string ErrorMessage,
    int? DurationMs,
    string WcagVersion,
    string WcagLevel
);

public record AnalysisPatchDto(
    string? ContentType,
    string? ContentInput,
    string? SourceUrl,
    string? ToolUsed,
    string? Status,
    string? SummaryResult,
    string? ResultJson,
    string? ErrorMessage,
    int? DurationMs,
    string? WcagVersion,
    string? WcagLevel
);