using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.UpdateSupplier;

public class UpdateSupplierCommand : IRequest<Result<SupplierDto>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? ContactName { get; init; }
    public string? ContactPhone { get; init; }
    public string? Address { get; init; }
    public bool IsActive { get; init; }
}