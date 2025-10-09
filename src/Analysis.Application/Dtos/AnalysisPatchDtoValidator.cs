using FluentValidation;

namespace Analysis.Application.Dtos;

public class AnalysisPatchDtoValidator : AbstractValidator<AnalysisPatchDto>
{
    public AnalysisPatchDtoValidator()
    {
        _ = RuleFor(x => x.WcagVersion).Must(v => v is null or "2.0" or "2.1" or "2.2");
        _ = RuleFor(x => x.WcagLevel).Must(l => l is null or "A" or "AA" or "AAA");
    }
}
