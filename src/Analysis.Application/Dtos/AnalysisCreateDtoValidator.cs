using FluentValidation;

namespace Analysis.Application.Dtos;

public class AnalysisCreateDtoValidator : AbstractValidator<AnalysisCreateDto>
{
    public AnalysisCreateDtoValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.DateAnalysis).NotEmpty();
        RuleFor(x => x.ContentType).NotEmpty();
        RuleFor(x => x.ContentInput).NotEmpty();
        RuleFor(x => x.SourceUrl).NotEmpty();
        RuleFor(x => x.ToolUsed).NotEmpty();
        RuleFor(x => x.Status).NotEmpty();
        RuleFor(x => x.WcagVersion).NotEmpty().Must(v => v == "2.0" || v == "2.1" || v == "2.2");
        RuleFor(x => x.WcagLevel).NotEmpty().Must(l => l == "A" || l == "AA" || l == "AAA");
    }
}