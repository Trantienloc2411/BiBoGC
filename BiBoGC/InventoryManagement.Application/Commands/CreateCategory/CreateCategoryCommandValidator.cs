using FluentValidation;

namespace InventoryManagement.Application.Commands.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên không được để trống.")
            .MaximumLength(100).WithMessage("Tên danh mục không được quá 100 ký tự.");
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả danh mục không được vượt quá 500 ký tự.");
        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Thứ tự hiển thị không phải là một số nguyên âm");
    }
}