using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using MediatR;
using Sale.Application.DTOs;
using Sale.Application.Interfaces;
using Sale.Application.Mappers;
using Shared.Application.Common;

namespace Sale.Application.Commands.CompleteOrder;

public class CompleteOrderCommandHandler : IRequestHandler<CompleteOrderCommand, Result<SalesOrderDto>>
{
    private readonly ISalesOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IStockTransactionRepository _stockTransactionRepository;
    private readonly ISaleUnitOfWork _unitOfWork;
    private readonly IProductVariantRepository _variantRepository;

    public CompleteOrderCommandHandler(
        ISalesOrderRepository orderRepository,
        IProductRepository productRepository,
        IProductVariantRepository variantRepository,
        IStockTransactionRepository stockTransactionRepository,
        ISaleUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _variantRepository = variantRepository;
        _stockTransactionRepository = stockTransactionRepository;
        _unitOfWork = unitOfWork;
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

        // Complete the order in memory (raises SalesOrderCompletedEvent)
        try
        {
            order.Complete(request.AmountPaid);
        }
        catch (InvalidOperationException ex)
        {
            return Result<SalesOrderDto>.Failure(ex.Message);
        }

        // Persist order state change atomically to SaleDb
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

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
                productBatchId: item.ProductBatchId ?? Guid.Empty,
                supplierId: Guid.Empty,
                transactionType: StockTransactionType.Sale,
                quantity: quantityInBaseUnits,
                unitPrice: item.UnitPrice / variant.QuantityBaseUnit,
                transactionDate: DateTime.UtcNow,
                notes: $"Bán hàng - Đơn hàng: {order.OrderNumber}, Variant: {item.VariantName}"
            );

            await _stockTransactionRepository.AddAsync(stockTransaction, cancellationToken);
        }

        return Result<SalesOrderDto>.Success(SalesOrderMapper.MapToDto(order));
    }
}