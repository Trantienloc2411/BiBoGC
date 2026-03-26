using InventoryManagement.Application.DTOs;
using MediatR;

namespace InventoryManagement.Application.Queries.GetExpiringSoonProducts;

/// <summary>
/// Query to get products that have batches expiring soon
/// </summary>
public record GetExpiringSoonProductsQuery : IRequest<IEnumerable<ProductDto>>
{
    /// <summary>
    /// Number of days to consider as "expiring soon" (default: 30)
    /// </summary>
    public int ThresholdDays { get; init; } = 30;
}
