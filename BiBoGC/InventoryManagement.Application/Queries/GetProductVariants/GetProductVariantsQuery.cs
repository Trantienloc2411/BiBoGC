using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetProductVariants;

public class GetProductVariantsQuery : IRequest<PagedResult<ProductVariantDto>>
{
    public int PageSize { get; init; } = 10;
    public int PageNumber { get; init; } = 1;
    public int TotalCount { get; init; }
    public string? SearchString { get; init; }
}