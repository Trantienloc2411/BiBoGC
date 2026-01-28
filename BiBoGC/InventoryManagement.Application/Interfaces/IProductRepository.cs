using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Interfaces;

/// <summary>
/// Repository interface for Product entity operations
/// Provides CRUD operations and specialized queries for products
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Get product by ID including batches
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product with batches or null if not found</returns>
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get product by ID without batches (for quick lookups)
    /// </summary>
    Task<Product?> GetByIdWithoutBatchesAsync(Guid id, CancellationToken cancellationToken = default);


    /// <summary>
    /// Get all products with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="searchTerm">Optional search term for name/SKU</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of (products list, total count)</returns>
    Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if SKU already exists
    /// </summary>
    /// <param name="sku">SKU to check</param>
    /// <param name="excludeProductId">Product ID to exclude from check (for updates)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if SKU exists</returns>
    Task<bool> SkuExistsAsync(string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new product
    /// </summary>
    /// <param name="product">Product to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="product">Product to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft delete a product
    /// </summary>
    /// <param name="product">Product to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get products with low stock (less than threshold)
    /// </summary>
    /// <param name="threshold">Stock threshold</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get products with expiring batches soon
    /// </summary>
    /// <param name="daysUntilExpiry">Days until expiry threshold</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IEnumerable<Product>> GetProductsWithExpiringSoonBatchesAsync(int daysUntilExpiry = 30,
        CancellationToken cancellationToken = default);
}