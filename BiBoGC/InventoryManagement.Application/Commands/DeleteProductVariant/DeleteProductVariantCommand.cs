using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.DeleteProductVariant;

public class DeleteProductVariantCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }   
}