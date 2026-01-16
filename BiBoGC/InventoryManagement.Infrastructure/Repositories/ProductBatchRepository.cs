using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories;

public class ProductBatchRepository : IProductBatchRepository
{
    private readonly InventoryDbContext _context;

    public ProductBatchRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<ProductBatch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ProductBatches
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
    }

    public async Task<ProductBatch?> GetByIdWithProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ProductBatches
            .Include(b => b.Product)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
    }

    public async Task<(IEnumerable<ProductBatch> Batches, int TotalCount)> GetByProductIdAsync(
        Guid productId,
        int pageNumber = 1,
        int pageSize = 10,
        bool includeExpired = false,
        string? sortBy = "ExpirationDate",
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ProductBatches
            .Where(b => b.ProductId == productId && !b.IsDeleted);

        if (!includeExpired)
        {
            query = query.Where(b => b.ExpirationDate > DateTime.UtcNow);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortBy?.ToLower() switch
        {
            "expirationdate" => sortDescending
                ? query.OrderByDescending(b => b.ExpirationDate)
                : query.OrderBy(b => b.ExpirationDate),
            "manufacturingdate" => sortDescending
                ? query.OrderByDescending(b => b.ManufacturingDate)
                : query.OrderBy(b => b.ManufacturingDate),
            "quantity" => sortDescending
                ? query.OrderByDescending(b => b.Quantity)
                : query.OrderBy(b => b.Quantity),
            "batchnumber" => sortDescending
                ? query.OrderByDescending(b => b.BatchNumber)
                : query.OrderBy(b => b.BatchNumber),
            "createdat" => sortDescending
                ? query.OrderByDescending(b => b.CreatedAt)
                : query.OrderBy(b => b.CreatedAt),
            _ => query.OrderBy(b => b.ExpirationDate) // Default: FEFO (First Expiry, First Out)
        };

        var batches = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (batches, totalCount);
    }

    public async Task<IEnumerable<ProductBatch>> GetExpiringSoonAsync(
        int daysUntilExpiry = 30,
        CancellationToken cancellationToken = default)
    {
        var warningDate = DateTime.UtcNow.AddDays(daysUntilExpiry);

        return await _context.ProductBatches
            .Include(b => b.Product)
            .Where(b => !b.IsDeleted &&
                        b.ExpirationDate > DateTime.UtcNow &&
                        b.ExpirationDate <= warningDate &&
                        b.Quantity > 0)
            .OrderBy(b => b.ExpirationDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductBatch>> GetExpiredAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ProductBatches
            .Include(b => b.Product)
            .Where(b => !b.IsDeleted &&
                        b.ExpirationDate <= DateTime.UtcNow &&
                        b.Quantity > 0)
            .OrderBy(b => b.ExpirationDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> BatchNumberExistsAsync(
        Guid productId,
        string batchNumber,
        Guid? excludeBatchId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedBatchNumber = batchNumber.Trim().ToUpper();

        var query = _context.ProductBatches
            .Where(b => b.ProductId == productId &&
                        b.BatchNumber == normalizedBatchNumber &&
                        !b.IsDeleted);

        if (excludeBatchId.HasValue)
        {
            query = query.Where(b => b.Id != excludeBatchId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<ProductBatch> AddAsync(ProductBatch batch, CancellationToken cancellationToken = default)
    {
        await _context.ProductBatches.AddAsync(batch, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return batch;
    }

    public async Task UpdateAsync(ProductBatch batch, CancellationToken cancellationToken = default)
    {
        _context.ProductBatches.Update(batch);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ProductBatch batch, CancellationToken cancellationToken = default)
    {
        batch.SoftDelete();
        _context.ProductBatches.Update(batch);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
