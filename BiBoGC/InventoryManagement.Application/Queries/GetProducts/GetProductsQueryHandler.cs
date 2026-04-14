using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using MediatR;

namespace InventoryManagement.Application.Queries.GetProducts;

/// <summary>
/// Handler for GetProductsQuery
/// </summary>
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedResult<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PaginatedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var (products, totalCount) = await _productRepository.GetAllAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.Status,
            request.CategoryId,
            request.SupplierId,
            request.IsLowStock,
            cancellationToken
        );

        var dtos = products.Select(MapToDto);

        return new PaginatedResult<ProductDto>
        {
            Items = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.SkuGeneral.Value,
            Price = product.BasePrice.Value,
            Currency = "VND",
            Description = product.Description,
            Status = product.Status.ToString(),
            CategoryId = product.CategoryId,
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