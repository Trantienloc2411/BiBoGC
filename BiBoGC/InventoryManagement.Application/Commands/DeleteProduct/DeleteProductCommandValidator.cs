using FluentValidation;

namespace InventoryManagement.Application.Commands.DeleteProduct;

/// <summary>
/// Validator for DeleteProductCommand
/// </summary>
public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID sản phẩm không được để trống.");
    }
}
