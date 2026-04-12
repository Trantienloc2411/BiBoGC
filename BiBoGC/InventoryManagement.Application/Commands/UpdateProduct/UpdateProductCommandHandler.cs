using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Enums;
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
        if (product == null) return Result<ProductDto>.Failure($"Không tìm thấy sản phẩm với ID '{request.Id}'.");

        // Update price if provided
        if (request.Price.HasValue) product.UpdatePrice(new Money(request.Price.Value));

        // Update status if provided
        if (request.Status.HasValue)
        {
            var newStatus = (ProductStatuses)request.Status.Value;
            if (newStatus != product.Status)
            {
                try
                {
                    switch (newStatus)
                    {
                        case ProductStatuses.Inactive:
                            product.SetInactive();
                            break;
                        case ProductStatuses.Discontinued:
                            product.Discontinue();
                            break;
                        case ProductStatuses.Active:
                            product.Reactivate();
                            break;
                        case ProductStatuses.OutOfStock:
                            return Result<ProductDto>.Failure("Không thể đặt thủ công trạng thái 'Hết hàng'. Trạng thái này được cập nhật tự động khi tồn kho về 0.");
                        default:
                            return Result<ProductDto>.Failure($"Trạng thái không hợp lệ: {request.Status.Value}.");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    return Result<ProductDto>.Failure(ex.Message);
                }
            }
        }

        await _productRepository.UpdateAsync(product, cancellationToken);

        // Map to DTO
        var dto = MapToDto(product);
        return Result<ProductDto>.Success(dto);
    }

    private static ProductDto MapToDto(Domain.Entities.Product product)
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