using FluentValidation;
using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Application.Commands.CreateStockTransaction;

public class CreateStockTransactionCommandValidator : AbstractValidator<CreateStockTransactionCommand>
{
    public CreateStockTransactionCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ID sản phẩm không được để trống.");

        RuleFor(x => x.TransactionType)
            .IsInEnum().WithMessage("Loại giao dịch không hợp lệ.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Đơn giá không được âm.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Ghi chú không được vượt quá 500 ký tự.");

        // Supplier is required for Purchase transactions
        RuleFor(x => x.SupplierId)
            .NotEmpty()
            .When(x => x.TransactionType == StockTransactionType.Purchase)
            .WithMessage("Nhà cung cấp là bắt buộc cho giao dịch nhập hàng.");
    }
}