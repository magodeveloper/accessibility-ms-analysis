using FluentValidation;

namespace Analysis.Application.Dtos;

public class AnalysisCreateDtoValidator : AbstractValidator<AnalysisCreateDto>
{
    public AnalysisCreateDtoValidator()
    {
        _ = RuleFor(x => x.UserId).GreaterThan(0);
        _ = RuleFor(x => x.DateAnalysis).NotEmpty();
        _ = RuleFor(x => x.ContentType).NotEmpty();
        _ = RuleFor(x => x.ContentInput).NotEmpty();
        _ = RuleFor(x => x.SourceUrl).NotEmpty();
        _ = RuleFor(x => x.ToolUsed).NotEmpty();
        _ = RuleFor(x => x.Status).NotEmpty();
        _ = RuleFor(x => x.WcagVersion).NotEmpty().Must(v => v is "2.0" or "2.1" or "2.2");
        _ = RuleFor(x => x.WcagLevel).NotEmpty().Must(l => l is "A" or "AA" or "AAA");
    }
}
