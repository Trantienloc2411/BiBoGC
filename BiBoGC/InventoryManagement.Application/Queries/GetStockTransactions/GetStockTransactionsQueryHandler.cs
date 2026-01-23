using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using MediatR;

namespace InventoryManagement.Application.Queries.GetStockTransactions;

public class GetStockTransactionsQueryHandler : IRequestHandler<GetStockTransactionsQuery, PaginatedResult<StockTransactionDto>>
{
    private readonly IStockTransactionRepository _stockTransactionRepository;

    public GetStockTransactionsQueryHandler(IStockTransactionRepository stockTransactionRepository)
    {
        _stockTransactionRepository = stockTransactionRepository;
    }

    public async Task<PaginatedResult<StockTransactionDto>> Handle(GetStockTransactionsQuery request, CancellationToken cancellationToken)
    {
        var (transactions, totalCount) = await _stockTransactionRepository.GetAllAsync(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            keyword: request.SearchTerm,
            cancellationToken: cancellationToken
        );

        var dtos = transactions.Select(t => new StockTransactionDto
        {
            Id = t.Id,
            ProductId = t.ProductId,
            ProductName = t.Product?.Name ?? string.Empty,
            ProductBatchId = t.ProductBatchId == Guid.Empty ? null : t.ProductBatchId,
            BatchNumber = t.ProductBatch?.BatchNumber,
            SupplierId = t.SupplierId == Guid.Empty ? null : t.SupplierId,
            SupplierName = t.Supplier?.Name,
            Sku = t.Product?.SkuGeneral ?? string.Empty,
            UnitPrice = t.UnitPrice,
            TotalAmount = t.TotalPrice,
            TransactionType = t.TransactionType.ToString(),
            TransactionDate = t.TransactionDate,
            Notes = t.Notes
        });

        return new PaginatedResult<StockTransactionDto>
        {
            Items = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
