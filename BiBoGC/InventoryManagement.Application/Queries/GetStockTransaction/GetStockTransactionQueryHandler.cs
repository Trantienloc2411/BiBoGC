using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetStockTransaction;

public class GetStockTransactionQueryHandler : IRequestHandler<GetStockTransactionQuery, Result<StockTransactionDto>>
{
    private readonly IStockTransactionRepository _stockTransactionRepository;

    public GetStockTransactionQueryHandler(IStockTransactionRepository stockTransactionRepository)
    {
        _stockTransactionRepository = stockTransactionRepository;
    }

    public async Task<Result<StockTransactionDto>> Handle(GetStockTransactionQuery request,
        CancellationToken cancellationToken)
    {
        var transaction = await _stockTransactionRepository.GetByIdAsync(request.Id, cancellationToken);

        if (transaction is null)
            return Result<StockTransactionDto>.Failure($"Không tìm thấy giao dịch kho với ID '{request.Id}'.");

        var dto = new StockTransactionDto
        {
            Id = transaction.Id,
            ProductId = transaction.ProductId,
            ProductName = transaction.Product?.Name ?? string.Empty,
            ProductBatchId = transaction.ProductBatchId == Guid.Empty ? null : transaction.ProductBatchId,
            BatchNumber = transaction.ProductBatch?.BatchNumber,
            SupplierId = transaction.SupplierId == Guid.Empty ? null : transaction.SupplierId,
            SupplierName = transaction.Supplier?.Name,
            Sku = transaction.Product?.SkuGeneral ?? string.Empty,
            Quantity = transaction.Quantity,
            UnitPrice = transaction.UnitPrice,
            TotalAmount = transaction.TotalPrice,
            TransactionType = transaction.TransactionType.ToString(),
            TransactionDate = transaction.TransactionDate,
            Notes = transaction.Notes
        };

        return Result<StockTransactionDto>.Success(dto);
    }
}