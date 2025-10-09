using FluentValidation;

namespace Analysis.Application.Dtos;

public class ResultCreateDtoValidator : AbstractValidator<ResultCreateDto>
{
    public ResultCreateDtoValidator()
    {
        _ = RuleFor(x => x.AnalysisId).GreaterThan(0);
        _ = RuleFor(x => x.WcagCriterionId).GreaterThan(0);
        _ = RuleFor(x => x.WcagCriterion).NotEmpty();
        _ = RuleFor(x => x.Level).NotEmpty();
        _ = RuleFor(x => x.Severity).NotEmpty();
        _ = RuleFor(x => x.Description).NotEmpty();
        // No hay reglas para Score ni Summary porque no existen en el DTO
    }
}
