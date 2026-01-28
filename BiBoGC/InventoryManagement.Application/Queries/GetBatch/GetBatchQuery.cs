using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetBatch;

public record GetBatchQuery : IRequest<Result<ProductBatchDto>>
{
    public Guid Id { get; init; }
}