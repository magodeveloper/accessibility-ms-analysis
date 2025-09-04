namespace Analysis.Application.Dtos;

public record ErrorDto(
    int Id,
    int ResultId,
    int WcagCriterionId,
    string ErrorCode,
    string Description,
    string Location,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ErrorCreateDto(
    int ResultId,
    int WcagCriterionId,
    string ErrorCode,
    string Description,
    string Location
);

public record ErrorPatchDto(
    string? ErrorCode,
    string? Description,
    string? Location
);