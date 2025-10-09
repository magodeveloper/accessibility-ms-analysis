using FluentValidation;

namespace Analysis.Application.Dtos;

public class ErrorCreateDtoValidator : AbstractValidator<ErrorCreateDto>
{
    public ErrorCreateDtoValidator()
    {
        _ = RuleFor(x => x.ResultId).GreaterThan(0);
        _ = RuleFor(x => x.WcagCriterionId).GreaterThan(0);
        _ = RuleFor(x => x.ErrorCode).NotEmpty();
        _ = RuleFor(x => x.Description).NotEmpty();
        _ = RuleFor(x => x.Location).NotEmpty();
    }
}
