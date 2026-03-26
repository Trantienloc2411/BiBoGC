using InventoryManagement.Application.DTOs;
using InventoryManagement.Domain.Enums;
using MediatR;

namespace InventoryManagement.Application.Queries.GetProducts;

/// <summary>
/// Query to get a paginated list of products
/// </summary>
/// <remarks>
/// Returns paginated product list with search capability.
/// Search applies to product name, SKU, and description.
/// Results are sorted by name alphabetically.
/// </remarks>
public record GetProductsQuery : IRequest<PaginatedResult<ProductDto>>
{
    /// <summary>
    /// Page number (1-based, default: 1)
    /// </summary>
    /// <example>1</example>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Number of items per page (default: 10, max: 100)
    /// </summary>
    /// <example>10</example>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Optional search term (searches name, SKU, description)
    /// </summary>
    /// <example>coca</example>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Optional filter by product status
    /// </summary>
    public ProductStatuses? Status { get; init; }
}