namespace InventoryManagement.Application.DTOs;

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