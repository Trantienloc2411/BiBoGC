using FluentValidation;

namespace InventoryManagement.Application.Commands.CreateProduct;

/// <summary>
/// Validator for CreateProductCommand
/// </summary>
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên sản phẩm không được để trống.")
            .MaximumLength(200).WithMessage("Tên sản phẩm không được quá 200 ký tự.");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("Mã SKU không được để trống.")
            .MaximumLength(20).WithMessage("Mã SKU không được quá 20 ký tự.")
            .Matches(@"^[A-Za-z0-9\-_]+$")
            .WithMessage("Mã SKU chỉ được chứa chữ cái, số, dấu gạch ngang và gạch dưới.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Giá sản phẩm phải lớn hơn hoặc bằng 0.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả sản phẩm không được quá 1000 ký tự.");
    }
}