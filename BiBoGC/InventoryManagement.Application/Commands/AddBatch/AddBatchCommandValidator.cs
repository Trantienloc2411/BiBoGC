using FluentValidation;

namespace InventoryManagement.Application.Commands.AddBatch;

/// <summary>
/// Validator for AddBatchCommand
/// </summary>
public class AddBatchCommandValidator : AbstractValidator<AddBatchCommand>
{
    public AddBatchCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ID sản phẩm không được để trống.");

        RuleFor(x => x.BatchNumber)
            .NotEmpty().WithMessage("Số lô không được để trống.")
            .MaximumLength(50).WithMessage("Số lô không được quá 50 ký tự.")
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("Số lô chỉ được chứa chữ cái, số, dấu gạch ngang và gạch dưới.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0.");

        RuleFor(x => x.ManufacturingDate)
            .NotEmpty().WithMessage("Ngày sản xuất không được để trống.")
            .LessThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Ngày sản xuất không được trong tương lai.");

        RuleFor(x => x.ExpirationDate)
            .NotEmpty().WithMessage("Ngày hết hạn không được để trống.")
            .GreaterThan(x => x.ManufacturingDate).WithMessage("Ngày hết hạn phải sau ngày sản xuất.");

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Giá nhập phải lớn hơn hoặc bằng 0.");
    }
}