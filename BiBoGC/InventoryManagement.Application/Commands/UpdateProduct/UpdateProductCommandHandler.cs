using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.ValueObjects;
using MediatR;
using Shared.Application.Common;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace InventoryManagement.Application.Commands.UpdateProduct;

/// <summary>
/// Handler for UpdateProductCommand
/// </summary>
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly INotificationService _notificationService;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        INotificationService notificationService)
    {
        _productRepository = productRepository;
        _notificationService = notificationService;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        // Get existing product
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null) return Result<ProductDto>.Failure($"Không tìm thấy sản phẩm với ID '{request.Id}'.");

        var defaultVariant = product.Variants.OrderBy(v => v.DisplayOrder).FirstOrDefault();
        var priceChanged = request.Price.HasValue && defaultVariant != null &&
                            request.Price.Value != defaultVariant.SalePrice.Value;
        var oldPrice = defaultVariant?.SalePrice.Value ?? 0m;

        // Update price of the default variant if provided
        if (request.Price.HasValue && defaultVariant != null)
            defaultVariant.UpdatePrice(new Money(request.Price.Value), defaultVariant.CostPrice);

        var discontinuedNow = false;

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
                            discontinuedNow = true;
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

        if (priceChanged)
        {
            await _notificationService.NotifyAsync(
                "Giá sản phẩm thay đổi",
                $"Sản phẩm '{product.Name}' đã đổi giá từ {oldPrice:N0}đ → {request.Price!.Value:N0}đ.",
                NotificationType.PriceChanged,
                NotificationRole.Seller,
                product.Id,
                "Product",
                cancellationToken);
        }

        if (discontinuedNow)
        {
            await _notificationService.NotifyAsync(
                "Sản phẩm ngừng kinh doanh",
                $"Sản phẩm '{product.Name}' đã được đánh dấu ngừng kinh doanh.",
                NotificationType.ProductDiscontinued,
                NotificationRole.Both,
                product.Id,
                "Product",
                cancellationToken);
        }

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
            Price = product.Variants.OrderBy(v => v.DisplayOrder).FirstOrDefault()?.SalePrice.Value ?? 0m,
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