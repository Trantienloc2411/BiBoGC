using FluentValidation;

namespace InventoryManagement.Application.Queries.GetProduct;

/// <summary>
/// Validator for GetProductQuery
/// </summary>
public class GetProductQueryValidator : AbstractValidator<GetProductQuery>
{
    public GetProductQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID sản phẩm không được để trống.");
    }
}