using InventoryManagement.Application.DTOs;
using InventoryManagement.Domain.Enums;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.CreateStockTransaction;

public record CreateStockTransactionCommand : IRequest<Result<StockTransactionDto>>
{
    public Guid ProductId { get; init; }
    public Guid? ProductBatchId { get; init; }
    public Guid? SupplierId { get; init; }
    public StockTransactionType TransactionType { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string? Notes { get; init; }
}