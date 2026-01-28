using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.DeleteBatch;

public record DeleteBatchCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
}