using FluentValidation;

namespace InventoryManagement.Application.Commands.UpdateProductVariant;

public class UpdateProductVariantCommandValidator : AbstractValidator<UpdateProductVariantCommand>
{
    public UpdateProductVariantCommandValidator()
    {
        RuleFor(x => x.QuantityBaseUnit)
            .GreaterThanOrEqualTo(1).WithMessage("Đơn vị của biến thể không thể nhỏ hơn 1.");
        RuleFor(x => x.CostPrice.Value)
            .GreaterThan(0)
            .WithMessage("Giá vốn không thể âm");
        RuleFor(x => x.SalePrice.Value)
            .GreaterThanOrEqualTo(x => x.CostPrice.Value)
            .WithMessage("Giá bán không thể bé hơn hoặc bằng giá gốc");
        RuleFor(x => x.VariantName)
            .NotEmpty()
            .WithMessage("Giá trị cập nhật không được để trống");
        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Giá trị sắp xếp hiển thị không được âm.");
    }
}