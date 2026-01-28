using FluentValidation;

namespace InventoryManagement.Application.Queries.GetBatch;

public class GetBatchQueryValidator : AbstractValidator<GetBatchQuery>
{
    public GetBatchQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID lô hàng không được để trống.");
    }
}