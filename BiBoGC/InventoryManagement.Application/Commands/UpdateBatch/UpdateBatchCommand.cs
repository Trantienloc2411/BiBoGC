using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.UpdateBatch;

public record UpdateBatchCommand : IRequest<Result<ProductBatchDto>>
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public int? Quantity { get; init; }
    public DateTime? ManufacturingDate { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public decimal? CostPrice { get; init; }
}
