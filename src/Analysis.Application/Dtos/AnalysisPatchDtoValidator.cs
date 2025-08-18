using FluentValidation;

namespace Analysis.Application.Dtos;

public class AnalysisPatchDtoValidator : AbstractValidator<AnalysisPatchDto>
{
    public AnalysisPatchDtoValidator()
    {
        RuleFor(x => x.WcagVersion).Must(v => v == null || v == "2.0" || v == "2.1" || v == "2.2");
        RuleFor(x => x.WcagLevel).Must(l => l == null || l == "A" || l == "AA" || l == "AAA");
    }
}