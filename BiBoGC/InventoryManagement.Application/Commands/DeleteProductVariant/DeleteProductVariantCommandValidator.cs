using FluentValidation;

namespace InventoryManagement.Application.Commands.DeleteProductVariant;

public class DeleteProductVariantCommandValidator : AbstractValidator<DeleteProductVariantCommand>
{
    public DeleteProductVariantCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .WithMessage("Giá trị ID không được để trống!");
    }
}