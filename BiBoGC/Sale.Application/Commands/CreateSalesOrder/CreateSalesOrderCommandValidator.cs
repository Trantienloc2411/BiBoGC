using FluentValidation;

namespace Sale.Application.Commands.CreateSalesOrder;

public class CreateSalesOrderCommandValidator : AbstractValidator<CreateSalesOrderCommand>
{
    public CreateSalesOrderCommandValidator()
    {
        RuleFor(x => x.PaymentMethod)
            .IsInEnum()
            .WithMessage("Phương thức thanh toán không hợp lệ.");

        RuleFor(x => x.CustomerName)
            .MaximumLength(200)
            .WithMessage("Tên khách hàng không được quá 200 ký tự.")
            .When(x => !string.IsNullOrEmpty(x.CustomerName));

        RuleFor(x => x.CustomerPhone)
            .MaximumLength(20)
            .WithMessage("Số điện thoại không được quá 20 ký tự.")
            .Matches(@"^[0-9\-\+\s]*$")
            .WithMessage("Số điện thoại không hợp lệ.")
            .When(x => !string.IsNullOrEmpty(x.CustomerPhone));
    }
}