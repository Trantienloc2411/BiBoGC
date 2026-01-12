using InventoryManagement.Application.DTOs;
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
}

/// <summary>
/// Paginated result wrapper
/// </summary>
public class PaginatedResult<T>
{
    /// <summary>
    /// Items in current page
    /// </summary>
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPrevious => PageNumber > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNext => PageNumber < TotalPages;
}
