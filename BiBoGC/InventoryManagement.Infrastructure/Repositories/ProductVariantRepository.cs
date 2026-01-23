using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.ValueObjects;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories;

public class ProductVariantRepository : IProductVariantRepository
{
    private readonly InventoryDbContext _context;

    public ProductVariantRepository(InventoryDbContext context)
    {
        _context = context;
    }


    public async Task<ProductVariant?> GetProductVariantBySkuUniqueAsync(Sku sku,
        CancellationToken cancellationToken = default)
    {
        var product = await _context.ProductVariants.Where(p => p.SkuUnique.Value.ToLower() == sku.Value.ToLower())
            .FirstOrDefaultAsync(cancellationToken);
        return product;
    }

    public async Task<(IEnumerable<ProductVariant>, int TotalCount)> GetAllAsync(int pageNumber = 1, int pageSize = 10,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ProductVariants.AsQueryable();

        IEnumerable<ProductVariant> productVariants = query;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            productVariants = await query.Where(p => p.Product != null && (p.SkuUnique.Value.ToLower().Contains(search) ||
                                                                     p.VariantName.Contains(search,
                                                                         StringComparison.CurrentCultureIgnoreCase) ||
                                                                     p.Product.Name.Contains(search,
                                                                         StringComparison.CurrentCultureIgnoreCase) ||
                                                                     p.Barcode.Contains(search,
                                                                         StringComparison.CurrentCultureIgnoreCase))
            ).ToListAsync(cancellationToken);
        }

        var totalCount = productVariants.Count();

        //Apply pagination
        var variants = productVariants.OrderBy(p => p.VariantName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (variants, totalCount);
    }

    public async Task<IEnumerable<ProductVariant>> GetProductVariantsByProductIdAsync(Guid productId,
        CancellationToken cancellationToken = default)
    {
        var productVariants = await _context.ProductVariants.Where(p => p.ProductId == productId).ToListAsync(cancellationToken);
        return productVariants;
    }

    public async Task<ProductVariant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var productVariant = await _context.ProductVariants.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        return productVariant;
    }


    public async Task<ProductVariant> AddAsync(ProductVariant productVariant,
        CancellationToken cancellationToken = default)
    {
        await _context.ProductVariants.AddAsync(productVariant, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return productVariant;
    }

    public async Task UpdateAsync(ProductVariant productVariant, CancellationToken cancellationToken = default)
    {
        _context.ProductVariants.Update(productVariant);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ProductVariant productVariant, CancellationToken cancellationToken = default)
    {
        productVariant.SoftDelete();
        _context.ProductVariants.Update(productVariant);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SkuExistsAsync(Guid productId, Sku sku, Guid? excludeVariantId = null,
        CancellationToken cancellationToken = default)
    {
        var existingVariant = await _context.ProductVariants.FirstOrDefaultAsync(p => p.ProductId == productId && p.SkuUnique == sku, cancellationToken);
        return existingVariant != null && existingVariant.Id != excludeVariantId;
    }
}