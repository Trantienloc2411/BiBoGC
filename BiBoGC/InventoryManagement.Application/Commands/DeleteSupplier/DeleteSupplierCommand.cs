using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.DeleteSupplier;

public record DeleteSupplierCommand : IRequest<Result>
{
    public Guid Id { get; init; }
}
