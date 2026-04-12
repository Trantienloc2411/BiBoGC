using FluentValidation;

namespace InventoryManagement.Application.Commands.UpdateProduct;

/// <summary>
/// Validator for UpdateProductCommand
/// </summary>
public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID sản phẩm không được để trống.");

        When(x => x.Name != null, () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên sản phẩm không được để trống khi cập nhật.")
                .MaximumLength(200).WithMessage("Tên sản phẩm không được quá 200 ký tự.");
        });

        When(x => x.Price.HasValue, () =>
        {
            RuleFor(x => x.Price!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Giá sản phẩm phải lớn hơn hoặc bằng 0.");
        });

        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Mô tả sản phẩm không được quá 1000 ký tự.");
        });

        When(x => x.Status.HasValue, () =>
        {
            RuleFor(x => x.Status!.Value)
                .Must(s => s >= 1 && s <= 4).WithMessage("Trạng thái sản phẩm phải là 1 (Đang bán), 2 (Ngừng bán), hoặc 3 (Ngừng kinh doanh).");
        });
    }
}