using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetStockTransaction;

public record GetStockTransactionQuery : IRequest<Result<StockTransactionDto>>
{
    public Guid Id { get; init; }
}