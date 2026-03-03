using FluentValidation;

namespace Sale.Application.Commands.AddItemToOrder;

public class AddItemToOrderCommandValidator : AbstractValidator<AddItemToOrderCommand>
{
    public AddItemToOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("ID đơn hàng không được để trống.");

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ID sản phẩm không được để trống.");

        RuleFor(x => x.ProductVariantId)
            .NotEmpty()
            .WithMessage("ID biến thể sản phẩm không được để trống.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Số lượng phải lớn hơn 0.");
    }
}