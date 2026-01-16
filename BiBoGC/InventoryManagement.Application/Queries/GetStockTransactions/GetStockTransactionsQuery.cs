using InventoryManagement.Application.DTOs;
using InventoryManagement.Domain.Enums;
using MediatR;

namespace InventoryManagement.Application.Queries.GetStockTransactions;

public record GetStockTransactionsQuery : IRequest<PaginatedResult<StockTransactionDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public Guid? ProductId { get; init; }
    public Guid? SupplierId { get; init; }
    public StockTransactionType? TransactionType { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? SortBy { get; init; } = "TransactionDate";
    public bool SortDescending { get; init; } = true;
}
