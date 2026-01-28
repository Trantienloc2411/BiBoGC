using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetProductVariantsByProductId;

public class GetProductVariantsByProductIdQuery : IRequest<Result<IEnumerable<ProductVariantDto>>>
{
    public Guid ProductId { get; init; }
}