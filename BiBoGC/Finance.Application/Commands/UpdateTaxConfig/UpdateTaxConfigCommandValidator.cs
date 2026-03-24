using FluentValidation;

namespace Finance.Application.Commands.UpdateTaxConfig;

public class UpdateTaxConfigCommandValidator : AbstractValidator<UpdateTaxConfigCommand>
{
    public UpdateTaxConfigCommandValidator()
    {
        RuleFor(x => x.Rate)
            .InclusiveBetween(0m, 1m)
            .WithMessage("Thuế suất phải nằm trong khoảng 0 – 1 (vd: 0.10 = 10%).");
    }
}