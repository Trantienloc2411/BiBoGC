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
using ITaxConfigService = Shared.Application.Interfaces.ITaxConfigService;

namespace Sale.Application.Commands.CompleteOrder;

public class CompleteOrderCommandHandler : IRequestHandler<CompleteOrderCommand, Result<SalesOrderDto>>
{
    private readonly IAuditLogger _auditLogger;
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
        ITaxConfigService taxConfigService)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _variantRepository = variantRepository;
        _stockTransactionRepository = stockTransactionRepository;
        _unitOfWork = unitOfWork;
        _auditLogger = auditLogger;
        _taxConfigService = taxConfigService;
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
                    $"Tồn: {availableStock} {product.BaseUnits}, Yêu cầu: {quantityInBaseUnits} {product.BaseUnits}");

            stockDeductions.Add((product, variant, batch, quantityInBaseUnits, item));
        }

        // Apply VAT before completing (rate = 0 when VAT is disabled)
        var vatRate = await _taxConfigService.GetActiveVatRateAsync(cancellationToken);
        order.ApplyTax(vatRate);

        // Complete the order in memory (raises SalesOrderCompletedEvent)
        try
        {
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
        // Order is now permanently Completed. If this phase fails, the caller receives a 500
        // and can retry; the retry will hit order.Complete() which throws for an already-completed
        // order, so double-completion is not possible.
        foreach (var (product, variant, batch, quantityInBaseUnits, item) in stockDeductions)
        {
            if (batch is not null)
                batch.DecreaseQuantity(quantityInBaseUnits);
            else
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
        }

        var dto = SalesOrderMapper.MapToDto(order);

        await _auditLogger.LogAsync(
            action: "SalesOrder.Complete",
            isSuccess: true,
            description:
            $"OrderId={order.Id}, OrderNumber={order.OrderNumber}, Total={order.TotalAmount:F2}, AmountPaid={request.AmountPaid:F2}",
            cancellationToken: cancellationToken);

        return Result<SalesOrderDto>.Success(dto);
    }
}