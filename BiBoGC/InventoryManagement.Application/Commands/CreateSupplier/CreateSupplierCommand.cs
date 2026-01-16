using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.CreateSupplier;

public record CreateSupplierCommand : IRequest<Result<SupplierDto>>
{
    public string Name { get; init; } = string.Empty;
    public string? ContactPerson { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Address { get; init; }
}
