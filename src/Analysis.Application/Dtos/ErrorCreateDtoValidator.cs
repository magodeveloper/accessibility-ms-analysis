using FluentValidation;

namespace Analysis.Application.Dtos;

public class ErrorCreateDtoValidator : AbstractValidator<ErrorCreateDto>
{
    public ErrorCreateDtoValidator()
    {
        RuleFor(x => x.ResultId).GreaterThan(0);
        RuleFor(x => x.WcagCriterionId).GreaterThan(0);
        RuleFor(x => x.ErrorCode).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Location).NotEmpty();
    }
}