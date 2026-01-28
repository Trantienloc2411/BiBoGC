using FluentValidation;

namespace InventoryManagement.Application.Commands.DeleteBatch;

public class DeleteBatchCommandValidator : AbstractValidator<DeleteBatchCommand>
{
    public DeleteBatchCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID lô hàng không được để trống.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ID sản phẩm không được để trống.");
    }
}