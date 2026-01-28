using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.DeleteProductVariant;

public record DeleteProductVariantCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
}