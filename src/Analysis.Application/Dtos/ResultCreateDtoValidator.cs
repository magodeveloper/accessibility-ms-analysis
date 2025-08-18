using FluentValidation;

namespace Analysis.Application.Dtos;

public class ResultCreateDtoValidator : AbstractValidator<ResultCreateDto>
{
    public ResultCreateDtoValidator()
    {
        RuleFor(x => x.AnalysisId).GreaterThan(0);
        RuleFor(x => x.WcagCriterionId).GreaterThan(0);
        RuleFor(x => x.WcagCriterion).NotEmpty();
        RuleFor(x => x.Level).NotEmpty();
        RuleFor(x => x.Severity).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        // No hay reglas para Score ni Summary porque no existen en el DTO
    }
}