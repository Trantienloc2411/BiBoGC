using FluentValidation;

namespace Sale.Application.Commands.UpdateOrderItemQuantity;

public class UpdateOrderItemQuantityCommandValidator : AbstractValidator<UpdateOrderItemQuantityCommand>
{
    public UpdateOrderItemQuantityCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("ID đơn hàng không được để trống.");

        RuleFor(x => x.ItemId)
            .NotEmpty()
            .WithMessage("ID sản phẩm không được để trống.");

        RuleFor(x => x.NewQuantity)
            .GreaterThan(0)
            .WithMessage("Số lượng phải lớn hơn 0.");
    }
}