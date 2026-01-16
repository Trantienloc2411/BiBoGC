using FluentValidation;

namespace InventoryManagement.Application.Commands.UpdateBatch;

public class UpdateBatchCommandValidator : AbstractValidator<UpdateBatchCommand>
{
    public UpdateBatchCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID lô hàng không được để trống.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ID sản phẩm không được để trống.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Quantity.HasValue)
            .WithMessage("Số lượng không được âm.");

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CostPrice.HasValue)
            .WithMessage("Giá vốn không được âm.");

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(x => x.ManufacturingDate)
            .When(x => x.ManufacturingDate.HasValue && x.ExpirationDate.HasValue)
            .WithMessage("Ngày hết hạn phải sau ngày sản xuất.");
    }
}
