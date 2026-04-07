using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using MediatR;
using Shared.Application.Common;
using Shared.Application.Interfaces;

namespace InventoryManagement.Application.Commands.CreateStockTransaction;

public class
    CreateStockTransactionCommandHandler : IRequestHandler<CreateStockTransactionCommand, Result<StockTransactionDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IStockTransactionRepository _stockTransactionRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IAuditLogger _auditLogger;

    public CreateStockTransactionCommandHandler(
        IStockTransactionRepository stockTransactionRepository,
        IProductRepository productRepository,
        ISupplierRepository supplierRepository,
        IAuditLogger auditLogger)
    {
        _stockTransactionRepository = stockTransactionRepository;
        _productRepository = productRepository;
        _supplierRepository = supplierRepository;
        _auditLogger = auditLogger;
    }

    public async Task<Result<StockTransactionDto>> Handle(CreateStockTransactionCommand request,
        CancellationToken cancellationToken)
    {
        // Validate Product exists
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result<StockTransactionDto>.Failure($"Không tìm thấy sản phẩm với ID '{request.ProductId}'.");

        // Validate Supplier if provided
        if (request.SupplierId.HasValue)
        {
            var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId.Value, cancellationToken);
            if (supplier is null)
                return Result<StockTransactionDto>.Failure(
                    $"Không tìm thấy nhà cung cấp với ID '{request.SupplierId}'.");
        }

        // Validate ProductBatch if provided
        ProductBatch? batch = null;
        if (request.ProductBatchId.HasValue)
        {
            batch = product.Batches.FirstOrDefault(b => b.Id == request.ProductBatchId.Value);
            if (batch is null)
                return Result<StockTransactionDto>.Failure(
                    $"Không tìm thấy lô hàng với ID '{request.ProductBatchId}' cho sản phẩm này.");
        }

        var transaction = new StockTransaction(
            request.ProductId,
            request.ProductBatchId,
            request.SupplierId,
            request.TransactionType,
            request.Quantity,
            request.UnitPrice,
            request.TransactionDate ?? DateTime.UtcNow,
            request.Notes
        );

        // Update product TotalStock and batch quantity
        if (transaction.IsInboundTransaction())
        {
            product.IncreaseStock(request.Quantity);
            // Purchase = initial receipt; batch quantity is already set correctly by AddBatch.
            // AdjustmentIn and Return act on existing batches, so they still need IncreaseQuantity.
            if (transaction.TransactionType != StockTransactionType.Purchase)
                batch?.IncreaseQuantity(request.Quantity);
        }
        else if (transaction.IsOutboundTransaction())
        {
            product.DecreaseStock(request.Quantity);
            batch?.DecreaseQuantity(request.Quantity);
        }

        await _productRepository.UpdateAsync(product, cancellationToken);

        var createdTransaction = await _stockTransactionRepository.AddAsync(transaction, cancellationToken);

        await _auditLogger.LogAsync(
            action: "StockTransaction.Create",
            isSuccess: true,
            description: $"Type={request.TransactionType}, ProductId={request.ProductId}, Qty={request.Quantity}, UnitPrice={request.UnitPrice:F2}",
            cancellationToken: cancellationToken);

        // Fetch the transaction with navigation properties
        var fullTransaction = await _stockTransactionRepository.GetByIdAsync(createdTransaction.Id, cancellationToken);

        var dto = new StockTransactionDto
        {
            Id = fullTransaction!.Id,
            ProductId = fullTransaction.ProductId,
            ProductName = fullTransaction.Product?.Name ?? string.Empty,
            ProductBatchId = fullTransaction.ProductBatchId == Guid.Empty ? null : fullTransaction.ProductBatchId,
            BatchNumber = fullTransaction.ProductBatch?.BatchNumber,
            SupplierId = fullTransaction.SupplierId == Guid.Empty ? null : fullTransaction.SupplierId,
            SupplierName = fullTransaction.Supplier?.Name,
            Sku = fullTransaction.Product?.SkuGeneral ?? string.Empty,
            UnitPrice = fullTransaction.UnitPrice,
            TotalAmount = fullTransaction.TotalPrice,
            TransactionType = fullTransaction.TransactionType.ToString(),
            TransactionDate = fullTransaction.TransactionDate,
            Notes = fullTransaction.Notes
        };

        return Result<StockTransactionDto>.Success(dto);
    }
}