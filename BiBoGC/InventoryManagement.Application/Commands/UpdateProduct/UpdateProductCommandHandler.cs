using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.ValueObjects;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.UpdateProduct;

/// <summary>
/// Handler for UpdateProductCommand
/// </summary>
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public UpdateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        // Get existing product
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            return Result<ProductDto>.Failure($"Không tìm thấy sản phẩm với ID '{request.Id}'.");
        }

        // Update price if provided
        if (request.Price.HasValue)
        {
            product.UpdatePrice(new Money(request.Price.Value));
        }

        // Note: Name and Description updates would need domain methods
        // For now, we'll save changes through repository
        await _productRepository.UpdateAsync(product, cancellationToken);

        // Map to DTO
        var dto = MapToDto(product);
        return Result<ProductDto>.Success(dto);
    }

    private static ProductDto MapToDto(InventoryManagement.Domain.Entities.Product product)
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
