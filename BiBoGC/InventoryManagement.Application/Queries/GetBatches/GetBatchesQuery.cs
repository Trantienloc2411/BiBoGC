using InventoryManagement.Application.DTOs;
using MediatR;

namespace InventoryManagement.Application.Queries.GetBatches;

public record GetBatchesQuery : IRequest<PaginatedResult<ProductBatchDto>>
{
    public Guid ProductId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public bool IncludeExpired { get; init; } = false;
    public string? SortBy { get; init; } = "ExpirationDate";
    public bool SortDescending { get; init; } = false;
}