using InventoryManagement.Application.DTOs;
using InventoryManagement.Domain.Enums;
using MediatR;

namespace InventoryManagement.Application.Queries.GetProducts;

/// <summary>
/// Query to get a paginated list of products
/// </summary>
/// <remarks>
/// Returns paginated product list with search and filter capability.
/// Search applies to product name, SKU, and description.
/// Results are sorted by newest created first.
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
    /// Optional filter by product status (Active=1, Inactive=2, Discontinued=3, OutOfStock=4)
    /// </summary>
    public ProductStatuses? Status { get; init; }

    /// <summary>
    /// Optional filter by category ID
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// Optional filter by supplier ID (products that have been supplied by this supplier)
    /// </summary>
    public Guid? SupplierId { get; init; }

    /// <summary>
    /// Optional filter: true = only products whose TotalStock is below their LowStockThreshold
    /// </summary>
    public bool? IsLowStock { get; init; }
}