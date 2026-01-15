using FluentValidation;

namespace InventoryManagement.Application.Queries.GetProducts;

/// <summary>
/// Validator for GetProductsQuery
/// </summary>
public class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang phải lớn hơn hoặc bằng 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Số lượng mỗi trang phải lớn hơn hoặc bằng 1.")
            .LessThanOrEqualTo(100).WithMessage("Số lượng mỗi trang không được quá 100.");
    }
}
