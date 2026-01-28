using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.ValueObjects;

namespace InventoryManagement.Application.Interfaces;

public interface IProductVariantRepository
{
    Task<ProductVariant?> GetProductVariantBySkuUniqueAsync(Sku sku, CancellationToken cancellationToken = default);

    Task<(IEnumerable<ProductVariant>, int TotalCount)> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        CancellationToken cancellationToken = default
    );

    Task<IEnumerable<ProductVariant>> GetProductVariantsByProductIdAsync(Guid productId,
        CancellationToken cancellationToken = default);


    Task<ProductVariant?> GetByIdAsync(Guid productId, Guid productVariantId,
        CancellationToken cancellationToken = default);

    Task<ProductVariant> AddAsync(ProductVariant productVariant, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductVariant productVariant, CancellationToken cancellationToken = default);
    Task DeleteAsync(ProductVariant productVariant, CancellationToken cancellationToken = default);

    Task<bool> SkuExistsAsync(Guid productId, Sku sku, Guid? excludeVariantId = null,
        CancellationToken cancellationToken = default);

    Task<bool> DoesVariantExistAsync(Guid productId, Guid? excludedVariantId,  int quantityBaseOnUnit, Units unit,
        CancellationToken cancellationToken = default);
}