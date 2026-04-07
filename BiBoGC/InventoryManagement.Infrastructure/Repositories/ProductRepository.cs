using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Product entity
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly InventoryDbContext _context;

    public ProductRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Batches.Where(b => !b.IsDeleted))
            .Include(p => p.Variants.Where(v => !v.IsDeleted))
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Product?> GetByIdWithoutBatchesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }


    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        ProductStatuses? status = null,
        CancellationToken cancellationToken = default)
    {
        // Load all products with batches
        var allProducts = await _context.Products
            .Include(p => p.Batches.Where(b => !b.IsDeleted))
            .Include(p => p.Variants.Where(v => !v.IsDeleted))
            .Include(p => p.Category)
            .ToListAsync(cancellationToken);

        IEnumerable<Product> filteredProducts = allProducts;

        // Apply search filter in memory (due to Value Object)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            filteredProducts = filteredProducts.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.Variants.Any(v => v.SkuUnique.Value.ToLower().Contains(search)) ||
                p.Description.ToLower().Contains(search));
        }

        if (status.HasValue)
            filteredProducts = filteredProducts.Where(p => p.Status == status.Value);

        // Get total count
        var totalCount = filteredProducts.Count();

        // Apply pagination
        var products = filteredProducts
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (products, totalCount);
    }

    public async Task<bool> SkuExistsAsync(string sku, Guid? excludeProductId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedSku = sku.Trim().ToUpper();

        // Load products and check in memory because of Value Object conversion
        var products = await _context.Products.Include(v => v.Variants.Where(v => !v.IsDeleted))
            .ToListAsync(cancellationToken);
        var query = products.Where(p => p.Variants.Any(v => v.SkuUnique.Value == normalizedSku) || p.SkuGeneral.Value.CompareTo(normalizedSku) == 0);

        if (excludeProductId.HasValue) query = query.Where(p => p.Id != excludeProductId.Value);

        return query.Any();
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        product.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold,
        CancellationToken cancellationToken = default)
    {
        var products = await _context.Products
            .Include(p => p.Batches.Where(b => !b.IsDeleted))
            .ToListAsync(cancellationToken);

        // Filter in memory because GetTotalStock involves DateTime calculations
        return products.Where(p => p.GetAvailableStock() < threshold);
    }

    public async Task<IEnumerable<Product>> GetProductsWithExpiringSoonBatchesAsync(int daysUntilExpiry = 30,
        CancellationToken cancellationToken = default)
    {
        var expiryThreshold = DateTime.UtcNow.AddDays(daysUntilExpiry);

        return await _context.Products
            .Include(p => p.Batches.Where(b => !b.IsDeleted))
            .Include(p => p.Category)
            .Where(p => p.Batches.Any(b =>
                !b.IsDeleted &&
                b.ExpirationDate <= expiryThreshold &&
                b.ExpirationDate > DateTime.UtcNow))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetProductsWithExpiredBatchesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Products
            .Include(p => p.Batches.Where(b => !b.IsDeleted))
            .Include(p => p.Category)
            .Where(p => p.Batches.Any(b => !b.IsDeleted && b.ExpirationDate < now))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetProductsWithLowStockAsync(CancellationToken cancellationToken = default)
    {
        var products = await _context.Products
            .Include(p => p.Batches.Where(b => !b.IsDeleted))
            .Include(p => p.Category)
            .ToListAsync(cancellationToken);

        return products.Where(p => p.LowStockThreshold.HasValue && p.GetAvailableStock() < p.LowStockThreshold.Value);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
