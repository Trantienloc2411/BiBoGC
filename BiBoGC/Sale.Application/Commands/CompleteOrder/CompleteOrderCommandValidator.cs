using FluentValidation;

namespace Sale.Application.Commands.CompleteOrder;

public class CompleteOrderCommandValidator : AbstractValidator<CompleteOrderCommand>
{
    public CompleteOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("ID đơn hàng không được để trống.");

        RuleFor(x => x.AmountPaid)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Số tiền thanh toán không được âm.");
    }
}