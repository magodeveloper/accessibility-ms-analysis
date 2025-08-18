namespace Analysis.Application.Dtos;

public record ResultDto(
    int Id,
    int AnalysisId,
    int WcagCriterionId,
    string WcagCriterion,
    string Level,
    string Severity,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ResultCreateDto(
    int AnalysisId,
    int WcagCriterionId,
    string WcagCriterion,
    string Level,
    string Severity,
    string Description
);

public record ResultPatchDto(
    string? WcagCriterion,
    string? Level,
    string? Severity,
    string? Description
);