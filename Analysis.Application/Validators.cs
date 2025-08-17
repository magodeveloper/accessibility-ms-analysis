using FluentValidation;
using Analysis.Application.Dtos;

namespace Analysis.Application;

public class AnalysisDtoValidator : AbstractValidator<AnalysisDto>
{
    public AnalysisDtoValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.ToolUsed).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Date).NotEmpty();
    }
}

public class ResultDtoValidator : AbstractValidator<ResultDto>
{
    public ResultDtoValidator()
    {
        RuleFor(x => x.AnalysisId).GreaterThan(0);
        RuleFor(x => x.Score).InclusiveBetween(0, 100);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(500);
    }
}

public class ErrorDtoValidator : AbstractValidator<ErrorDto>
{
    public ErrorDtoValidator()
    {
        RuleFor(x => x.ResultId).GreaterThan(0);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
    }
}