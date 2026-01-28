using FluentValidation;

namespace InventoryManagement.Application.Commands.DeleteCategory;

public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Mã danh mục không được để trống.")
            .Must(id => id != Guid.Empty).WithMessage("Mã danh mục không hợp lệ.");
    }
}