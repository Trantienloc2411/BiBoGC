using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetProduct;

/// <summary>
/// Query to get a single product by ID
/// </summary>
/// <remarks>
/// Returns full product details including batches.
/// Batches are limited to the 5 most recent.
/// </remarks>
public record GetProductQuery : IRequest<Result<ProductDto>>
{
    /// <summary>
    /// Product ID to retrieve (required)
    /// </summary>
    public Guid Id { get; init; }
}
