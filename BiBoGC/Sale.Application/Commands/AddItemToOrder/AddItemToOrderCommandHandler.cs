using InventoryManagement.Application.Interfaces;
using MediatR;
using Sale.Application.DTOs;
using Sale.Application.Interfaces;
using Sale.Application.Mappers;
using Shared.Application.Common;

namespace Sale.Application.Commands.AddItemToOrder;

public class AddItemToOrderCommandHandler : IRequestHandler<AddItemToOrderCommand, Result<SalesOrderDto>>
{
    private readonly ISalesOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IProductVariantRepository _variantRepository;

    public AddItemToOrderCommandHandler(
        ISalesOrderRepository orderRepository,
        IProductRepository productRepository,
        IProductVariantRepository variantRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _variantRepository = variantRepository;
    }

    public async Task<Result<SalesOrderDto>> Handle(
        AddItemToOrderCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load order
        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result<SalesOrderDto>.Failure($"Không tìm thấy đơn hàng với ID '{request.OrderId}'.");

        // 2. Load product
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result<SalesOrderDto>.Failure($"Không tìm thấy sản phẩm với ID '{request.ProductId}'.");

        // 3. Load variant (bắt buộc phải có variant)
        var variant =
            await _variantRepository.GetByIdAsync(request.ProductId, request.ProductVariantId, cancellationToken);
        if (variant is null)
            return Result<SalesOrderDto>.Failure(
                $"Không tìm thấy biến thể sản phẩm với ID '{request.ProductVariantId}'.");

        // 4. Auto-select batch if batch tracking enabled and no batch specified
        Guid? batchId = request.ProductBatchId;
        if (product.RequiresBatchTracking && !batchId.HasValue)
        {
            var availableBatches = product.GetBatchesForSale(request.Quantity);
            if (availableBatches.Any())
            {
                batchId = availableBatches.First().Id;
            }
        }

        // 5. Add item to order using variant info
        try
        {
            order.AddItem(
                productId: product.Id,
                productVariantId: variant.Id,
                productBatchId: batchId,
                productName: product.Name,
                variantName: variant.VariantName,
                sku: variant.SkuUnique.Value,
                unit: variant.Unit.ToString(),
                quantity: request.Quantity,
                unitPrice: variant.SalePrice.Value
            );
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return Result<SalesOrderDto>.Failure(ex.Message);
        }

        // 6. Save
        await _orderRepository.UpdateAsync(order, cancellationToken);

        return Result<SalesOrderDto>.Success(SalesOrderMapper.MapToDto(order));
    }
}