using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryManagement.Application.Commands.UpdateSupplier
{
    public class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
    {
        public UpdateSupplierCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Supplier ID is required.");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Supplier name is required.")
                                .MaximumLength(200).WithMessage("Supplier name must not exceed 200 characters.");
            RuleFor(x => x.ContactName).MaximumLength(100).WithMessage("Contact name must not exceed 100 characters.");
            RuleFor(x => x.ContactPhone).MaximumLength(10).WithMessage("Contact phone must not exceed 10 characters.");
            RuleFor(x => x.Address).MaximumLength(300).WithMessage("Address must not exceed 300 characters.");
        }
    }
}
