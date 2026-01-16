using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.CreateStockTransaction;

public class CreateStockTransactionCommandHandler : IRequestHandler<CreateStockTransactionCommand, Result<StockTransactionDto>>
{
    private readonly IStockTransactionRepository _stockTransactionRepository;
    private readonly IProductRepository _productRepository;
    private readonly ISupplierRepository _supplierRepository;

    public CreateStockTransactionCommandHandler(
        IStockTransactionRepository stockTransactionRepository,
        IProductRepository productRepository,
        ISupplierRepository supplierRepository)
    {
        _stockTransactionRepository = stockTransactionRepository;
        _productRepository = productRepository;
        _supplierRepository = supplierRepository;
    }

    public async Task<Result<StockTransactionDto>> Handle(CreateStockTransactionCommand request, CancellationToken cancellationToken)
    {
        // Validate Product exists
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result<StockTransactionDto>.Failure($"Không tìm thấy sản phẩm với ID '{request.ProductId}'.");
        }

        // Validate Supplier if provided
        if (request.SupplierId.HasValue)
        {
            var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId.Value, cancellationToken);
            if (supplier is null)
            {
                return Result<StockTransactionDto>.Failure($"Không tìm thấy nhà cung cấp với ID '{request.SupplierId}'.");
            }
        }

        // Validate ProductBatch if provided
        ProductBatch? batch = null;
        if (request.ProductBatchId.HasValue)
        {
            batch = product.Batches.FirstOrDefault(b => b.Id == request.ProductBatchId.Value);
            if (batch is null)
            {
                return Result<StockTransactionDto>.Failure($"Không tìm thấy lô hàng với ID '{request.ProductBatchId}' cho sản phẩm này.");
            }
        }

        var transaction = new StockTransaction(
            productId: request.ProductId,
            productBatchId: request.ProductBatchId ?? Guid.Empty,
            supplierId: request.SupplierId ?? Guid.Empty,
            transactionType: request.TransactionType,
            quantity: request.Quantity,
            unitPrice: request.UnitPrice,
            transactionDate: DateTime.UtcNow,
            notes: request.Notes
        );

        // Update batch quantity if batch is specified
        if (batch is not null)
        {
            if (transaction.IsInboundTransaction())
            {
                batch.IncreaseQuantity(request.Quantity);
            }
            else if (transaction.IsOutboundTransaction())
            {
                batch.DecreaseQuantity(request.Quantity);
            }

            await _productRepository.UpdateAsync(product, cancellationToken);
        }

        var createdTransaction = await _stockTransactionRepository.AddAsync(transaction, cancellationToken);

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
            Sku = fullTransaction.Product?.Sku ?? string.Empty,
            UnitPrice = fullTransaction.UnitPrice,
            TotalAmount = fullTransaction.TotalPrice,
            TransactionType = fullTransaction.TransactionType.ToString(),
            TransactionDate = fullTransaction.TransactionDate,
            Notes = fullTransaction.Notes
        };

        return Result<StockTransactionDto>.Success(dto);
    }
}
