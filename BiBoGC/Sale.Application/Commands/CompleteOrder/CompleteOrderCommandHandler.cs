using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using MediatR;
using Sale.Application.DTOs;
using Sale.Application.Interfaces;
using Sale.Application.Mappers;
using Sale.Domain.Exceptions;
using Shared.Application.Common;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using ITaxConfigService = Shared.Application.Interfaces.ITaxConfigService;

namespace Sale.Application.Commands.CompleteOrder;

public class CompleteOrderCommandHandler : IRequestHandler<CompleteOrderCommand, Result<SalesOrderDto>>
{
    private readonly IAuditLogger _auditLogger;
    private readonly INotificationService _notificationService;
    private readonly ISalesOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IStockTransactionRepository _stockTransactionRepository;
    private readonly ITaxConfigService _taxConfigService;
    private readonly ISaleUnitOfWork _unitOfWork;
    private readonly IProductVariantRepository _variantRepository;

    public CompleteOrderCommandHandler(
        ISalesOrderRepository orderRepository,
        IProductRepository productRepository,
        IProductVariantRepository variantRepository,
        IStockTransactionRepository stockTransactionRepository,
        ISaleUnitOfWork unitOfWork,
        IAuditLogger auditLogger,
        ITaxConfigService taxConfigService,
        INotificationService notificationService)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _variantRepository = variantRepository;
        _stockTransactionRepository = stockTransactionRepository;
        _unitOfWork = unitOfWork;
        _auditLogger = auditLogger;
        _taxConfigService = taxConfigService;
        _notificationService = notificationService;
    }

    public async Task<Result<SalesOrderDto>> Handle(
        CompleteOrderCommand request,
        CancellationToken cancellationToken)
    {
        // --- Phase 1: Validate (InventoryDb reads) + commit order state to SaleDb ---

        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result<SalesOrderDto>.Failure($"Không tìm thấy đơn hàng với ID '{request.OrderId}'.");

        // Validate stock and prepare deductions (InventoryDb reads only — no writes yet)
        var stockDeductions = new List<(
            Product Product,
            ProductVariant Variant,
            ProductBatch? Batch,
            int Quantity,
            Sale.Domain.Domain.SalesOrderItem Item)>();

        foreach (var item in order.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is null)
                return Result<SalesOrderDto>.Failure(
                    $"Sản phẩm '{item.ProductName}' không còn tồn tại trong hệ thống.");

            var variant =
                await _variantRepository.GetByIdAsync(item.ProductId, item.ProductVariantId, cancellationToken);
            if (variant is null)
                return Result<SalesOrderDto>.Failure(
                    $"Biến thể sản phẩm '{item.VariantName}' không còn tồn tại trong hệ thống.");

            var quantityInBaseUnits = item.Quantity * variant.QuantityBaseUnit;

            int availableStock;
            ProductBatch? batch = null;

            if (item.ProductBatchId.HasValue)
            {
                batch = product.Batches.FirstOrDefault(b => b.Id == item.ProductBatchId.Value);
                if (batch is null)
                    return Result<SalesOrderDto>.Failure(
                        $"Lô hàng của sản phẩm '{item.ProductName}' không còn tồn tại.");
                availableStock = batch.Quantity;
            }
            else
            {
                availableStock = product.GetAvailableStock();
            }

            if (availableStock < quantityInBaseUnits)
                return Result<SalesOrderDto>.Failure(
                    $"Sản phẩm '{item.ProductName} - {item.VariantName}' không đủ tồn kho. " +
                    $"Tồn: {availableStock} {product.BaseUnits.ToString()}, Yêu cầu: {quantityInBaseUnits} {product.BaseUnits.ToString()}");

            stockDeductions.Add((product, variant, batch, quantityInBaseUnits, item));
        }

        // Extract embedded tax for accounting/invoice purposes (does NOT change TotalAmount)
        var vatRate = await _taxConfigService.GetActiveVatRateAsync(cancellationToken);

        // Complete the order in memory (raises SalesOrderCompletedEvent)
        try
        {
            order.ApplyTax(vatRate);
            order.Complete(request.AmountPaid);
        }
        catch (InvalidOrderStateException ex)
        {
            return Result<SalesOrderDto>.Failure(ex.Message);
        }

        // Persist order state change atomically to SaleDb
        await _unitOfWork.ExecuteInTransactionAsync(
            async () => await _orderRepository.UpdateAsync(order, cancellationToken),
            cancellationToken);

        // --- Phase 2: Deduct stock in InventoryDb (after SaleDb commit) ---
        try
        {
            foreach (var (product, variant, batch, quantityInBaseUnits, item) in stockDeductions)
            {
                if (batch is not null)
                    batch.DecreaseQuantity(quantityInBaseUnits);
                else if (product.RequiresBatchTracking)
                    product.DeductFromBatches(quantityInBaseUnits);

                product.DecreaseStock(quantityInBaseUnits);

                await _productRepository.UpdateAsync(product, cancellationToken);

                var stockTransaction = new StockTransaction(
                    productId: item.ProductId,
                    item.ProductBatchId,
                    null,
                    transactionType: StockTransactionType.Sale,
                    quantity: quantityInBaseUnits,
                    unitPrice: item.UnitPrice / variant.QuantityBaseUnit,
                    transactionDate: DateTime.UtcNow,
                    notes: $"Bán hàng - Đơn hàng: {order.OrderNumber}, Variant: {item.VariantName}"
                );

                await _stockTransactionRepository.AddAsync(stockTransaction, cancellationToken);

                await NotifyStockAlertAsync(product, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // Stock deduction failed — revert the order back to Draft so it can be retried
            order.RevertToDraft();
            await _unitOfWork.ExecuteInTransactionAsync(
                async () => await _orderRepository.UpdateAsync(order, cancellationToken),
                cancellationToken);

            return Result<SalesOrderDto>.Failure(
                $"Trừ tồn kho thất bại: {ex.Message} — Đơn hàng đã được hoàn tác về trạng thái Nháp.");
        }

        var dto = SalesOrderMapper.MapToDto(order);

        await _auditLogger.LogAsync(
            action: "SalesOrder.Complete",
            isSuccess: true,
            description:
            $"OrderId={order.Id}, OrderNumber={order.OrderNumber}, Total={order.TotalAmount:F2}, AmountPaid={request.AmountPaid:F2}",
            cancellationToken: cancellationToken);

        await _notificationService.NotifyAsync(
            "Đơn hàng hoàn thành",
            $"Đơn hàng {order.OrderNumber} đã được thanh toán thành công. Tổng tiền: {order.TotalAmount:N0}đ.",
            NotificationType.OrderCompleted,
            NotificationRole.Both,
            order.Id,
            "SalesOrder",
            cancellationToken);

        return Result<SalesOrderDto>.Success(dto);
    }

    private async Task NotifyStockAlertAsync(Product product, CancellationToken cancellationToken)
    {
        if (product.Status == ProductStatuses.OutOfStock)
        {
            await _notificationService.NotifyAsync(
                "Sản phẩm hết hàng",
                $"Sản phẩm '{product.Name}' đã hết hàng.",
                NotificationType.OutOfStock,
                NotificationRole.Admin,
                product.Id,
                "Product",
                cancellationToken);
        }
        else if (product.IsLowStock())
        {
            await _notificationService.NotifyAsync(
                "Sắp hết hàng",
                $"Sản phẩm '{product.Name}' sắp hết hàng. Tồn kho hiện tại: {product.TotalStock} (ngưỡng: {product.LowStockThreshold}).",
                NotificationType.LowStock,
                NotificationRole.Admin,
                product.Id,
                "Product",
                cancellationToken);
        }
    }
}