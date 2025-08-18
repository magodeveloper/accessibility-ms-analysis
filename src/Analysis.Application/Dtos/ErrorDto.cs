namespace Analysis.Application.Dtos;

public record ErrorDto(
    int Id,
    int ResultId,
    int WcagCriterionId,
    string ErrorCode,
    string Description,
    string Location,
    string Message,
    string Code,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ErrorCreateDto(
    int ResultId,
    int WcagCriterionId,
    string ErrorCode,
    string Description,
    string Location,
    string Message,
    string Code
);

public record ErrorPatchDto(
    string? ErrorCode,
    string? Description,
    string? Location,
    string? Message,
    string? Code
);