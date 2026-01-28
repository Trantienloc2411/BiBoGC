using FluentValidation;

namespace InventoryManagement.Application.Commands.CreateSupplier;

public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên nhà cung cấp không được để trống.")
            .MaximumLength(200).WithMessage("Tên nhà cung cấp không được vượt quá 200 ký tự.");

        RuleFor(x => x.ContactPerson)
            .MaximumLength(100).WithMessage("Tên người liên hệ không được vượt quá 100 ký tự.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Số điện thoại không được vượt quá 20 ký tự.")
            .Matches(@"^[\d\s\-\+\(\)]*$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Số điện thoại không hợp lệ.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Địa chỉ không được vượt quá 500 ký tự.");
    }
}