using FluentValidation;

namespace InventoryManagement.Application.Queries.GetStockTransaction;

public class GetStockTransactionQueryValidator : AbstractValidator<GetStockTransactionQuery>
{
    public GetStockTransactionQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID giao dịch không được để trống.");
    }
}