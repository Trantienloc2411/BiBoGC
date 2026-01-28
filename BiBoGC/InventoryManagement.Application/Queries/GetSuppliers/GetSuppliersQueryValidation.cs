using FluentValidation;

namespace InventoryManagement.Application.Queries.GetSuppliers;

public class GetSuppliersQueryValidation : AbstractValidator<GetSuppliersQuery>
{
    public GetSuppliersQueryValidation()
    {
        RuleFor(s => s.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang phải lớn hơn hoặc bằng 1.");

        RuleFor(s => s.SortBy)
            .Matches("^(Name|CreatedAt)?$")
            .WithMessage("Trường sắp xếp không hợp lệ. Chỉ chấp nhận 'Name' hoặc 'CreatedAt'.");
    }
}