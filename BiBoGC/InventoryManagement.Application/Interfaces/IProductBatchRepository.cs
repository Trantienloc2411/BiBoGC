using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Interfaces;

/// <summary>
/// Repository interface for ProductBatch entity operations
/// </summary>
public interface IProductBatchRepository
{
    /// <summary>
    /// Get batch by ID
    /// </summary>
    Task<ProductBatch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get batch by ID with product details
    /// </summary>
    Task<ProductBatch?> GetByIdWithProductAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all batches for a specific product with pagination
    /// </summary>
    Task<(IEnumerable<ProductBatch> Batches, int TotalCount)> GetByProductIdAsync(
        Guid productId,
        int pageNumber = 1,
        int pageSize = 10,
        bool includeExpired = false,
        string? sortBy = "ExpirationDate",
        bool sortDescending = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get batches expiring soon
    /// </summary>
    Task<IEnumerable<ProductBatch>> GetExpiringSoonAsync(
        int daysUntilExpiry = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get expired batches
    /// </summary>
    Task<IEnumerable<ProductBatch>> GetExpiredAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if batch number exists for a product
    /// </summary>
    Task<bool> BatchNumberExistsAsync(
        Guid productId,
        string batchNumber,
        Guid? excludeBatchId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new batch
    /// </summary>
    Task<ProductBatch> AddAsync(ProductBatch batch, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a batch
    /// </summary>
    Task UpdateAsync(ProductBatch batch, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a batch (soft delete)
    /// </summary>
    Task DeleteAsync(ProductBatch batch, CancellationToken cancellationToken = default);
}
