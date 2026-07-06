using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.ValueObjects;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.CreateProduct;

/// <summary>
/// Handler for CreateProductCommand
/// Creates a new product in the inventory system
/// </summary>
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public CreateProductCommandHandler(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Check if SKU already exists
        if (await _productRepository.SkuExistsAsync(request.Sku, cancellationToken: cancellationToken))
            return Result<ProductDto>.Failure($"SKU '{request.Sku}' đã tồn tại trong hệ thống.");

        // Validate CategoryId if provided
        if (request.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken);
            if (category is null)
                return Result<ProductDto>.Failure($"Danh mục với ID '{request.CategoryId}' không tồn tại.");
        }

        // Create domain entity
        var product = new Product(
            request.Name,
            skuGeneral: new Sku(request.Sku),
            baseUnits: request.BaseUnits,
            description: request.Description,
            status: ProductStatuses.Active,
            requiresBatchTracking: request.RequiresBatchTracking,
            lowStockThreshold: request.LowStockThreshold,
            categoryId: request.CategoryId
        );

        // Add a default variant (1 base unit) automatically
        product.AddProductVariant(
            $"{request.Sku}-DEFAULT",
            product.Id,
            $"{request.Name} 1 {request.BaseUnits}",
            request.BaseUnits,
            1,
            new Money(request.Price),
            1,
            null,
            null
        );

        // Save to database
        await _productRepository.AddAsync(product, cancellationToken);

        // Map to DTO and return
        var dto = MapToDto(product);
        return Result<ProductDto>.Success(dto);
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
            CategoryId = product.CategoryId,
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