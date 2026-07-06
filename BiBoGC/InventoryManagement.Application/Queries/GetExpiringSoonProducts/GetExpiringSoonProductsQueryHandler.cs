using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using MediatR;

namespace InventoryManagement.Application.Queries.GetExpiringSoonProducts;

public class GetExpiringSoonProductsQueryHandler : IRequestHandler<GetExpiringSoonProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetExpiringSoonProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetExpiringSoonProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetProductsWithExpiringSoonBatchesAsync(request.ThresholdDays, cancellationToken);
        return products.Select(MapToDto);
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.SkuGeneral.Value,
            Price = product.Variants.OrderBy(v => v.DisplayOrder).FirstOrDefault()?.SalePrice.Value ?? 0m,
            Currency = "VND",
            Description = product.Description,
            Status = product.Status.ToString(),
            CategoryName = product.Category?.Name,
            LowStockThreshold = product.LowStockThreshold,
            RequiresBatchTracking = product.RequiresBatchTracking,
            TotalStock = product.TotalStock,
            AvailableStock = product.GetAvailableStock(),
            ExpiredStock = product.GetExpiredBatches().Sum(b => b.Quantity),
            ExpiringSoonStock = product.GetExpiringSoonBatches().Sum(b => b.Quantity),
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            RecentBatches = product.Batches
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .Select(ProductBatchDto.FromEntity)
        };
    }
}
