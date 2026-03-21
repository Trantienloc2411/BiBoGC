using FluentValidation;

namespace Finance.Application.Commands.CreateExpense;

public class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator()
    {
        RuleFor(x => x.Category)
            .IsInEnum()
            .WithMessage("Danh mục chi phí không hợp lệ.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Số tiền phải lớn hơn 0.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Mô tả không được để trống.")
            .MaximumLength(500)
            .WithMessage("Mô tả không được quá 500 ký tự.");

        RuleFor(x => x.ExpenseDate)
            .NotEmpty()
            .WithMessage("Ngày chi phí không được để trống.")
            .LessThanOrEqualTo(_ => DateTime.UtcNow.AddMinutes(5))
            .WithMessage("Ngày chi phí không được ở tương lai.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum()
            .WithMessage("Phương thức thanh toán không hợp lệ.");

        RuleFor(x => x.ReceiptNumber)
            .MaximumLength(100)
            .WithMessage("Số biên lai không được quá 100 ký tự.")
            .When(x => !string.IsNullOrEmpty(x.ReceiptNumber));
    }
}
