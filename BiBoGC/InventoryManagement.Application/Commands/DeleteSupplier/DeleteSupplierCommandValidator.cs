using FluentValidation;

namespace InventoryManagement.Application.Commands.DeleteSupplier;

public class DeleteSupplierCommandValidator : AbstractValidator<DeleteSupplierCommand>
{
    public DeleteSupplierCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID nhà cung cấp không được để trống.");
    }
}