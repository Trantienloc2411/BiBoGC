using FluentValidation;

namespace InventoryManagement.Application.Queries.GetBatches;

public class GetBatchesQueryValidator : AbstractValidator<GetBatchesQuery>
{
    public GetBatchesQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ID sản phẩm không được để trống.");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang phải lớn hơn hoặc bằng 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100.");
    }
}
