using FluentValidation;
using InventoryManagement.Application.Commands.AddProductVariant;

namespace InventoryManagement.Application.Commands.CreateProductVariant;

public class CreateProductVariantCommandValidator : AbstractValidator<CreateProductVariantCommand>
{
    public CreateProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithErrorCode("NULLVALUE")
            .WithMessage("Mã sản phẩm đầu vào không được để trống");
        RuleFor(x => x.QuantityBaseUnit)
            .GreaterThan(0).WithErrorCode("NONNEGATIVE")
            .WithMessage("Số lượng đơn vị không được là số âm")
            ;
        RuleFor(x => x.CostPrice)
            .GreaterThan(0).WithErrorCode("NONNEGATIVE")
            .WithMessage("Giá bán không được âm!");
        RuleFor(x => x.SalePrice)
            .GreaterThanOrEqualTo(x => x.CostPrice)
            .WithErrorCode("NONLOWERTHANCOSTPRICE")
            .WithMessage("Giá bán phải lớn hơn hoặc bằng giá vốn! Vui lòng chỉnh sửa giá vốn trước!");

        RuleFor(x => x.VariantName)
            .NotEmpty().WithErrorCode("NULLVALUE")
            .WithMessage("Trường tên biến thể không được trống.");
    }
}