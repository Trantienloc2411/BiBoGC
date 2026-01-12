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

    public CreateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Check if SKU already exists
        if (await _productRepository.SkuExistsAsync(request.Sku, cancellationToken: cancellationToken))
        {
            return Result<ProductDto>.Failure($"SKU '{request.Sku}' đã tồn tại trong hệ thống.");
        }

        // Create domain entity
        var product = new Product(
            name: request.Name,
            sku: new Sku(request.Sku),
            price: new Money(request.Price),
            description: request.Description,
            status: ProductStatuses.Active,
            requiresBatchTracking: request.RequiresBatchTracking
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
            Sku = product.Sku.Value,
            Price = product.Price.Value,
            Currency = "VND",
            Description = product.Description,
            Status = product.Status.ToString(),
            RequiresBatchTracking = product.RequiresBatchTracking,
            TotalStock = product.GetTotalStock(),
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
