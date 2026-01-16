using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.CreateSupplier;

public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, Result<SupplierDto>>
{
    private readonly ISupplierRepository _supplierRepository;

    public CreateSupplierCommandHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<Result<SupplierDto>> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        // Check if supplier name already exists
        if (await _supplierRepository.NameExistsAsync(request.Name, null, cancellationToken))
        {
            return Result<SupplierDto>.Failure($"Nhà cung cấp với tên '{request.Name}' đã tồn tại.");
        }

        var supplier = new Supplier(
            name: request.Name,
            contactPerson: request.ContactPerson,
            phoneNumber: request.PhoneNumber,
            address: request.Address
        );

        var createdSupplier = await _supplierRepository.AddAsync(supplier, cancellationToken);

        var dto = new SupplierDto
        {
            Id = createdSupplier.Id,
            Name = createdSupplier.Name,
            ContactName = createdSupplier.ContactPerson,
            ContactPhone = createdSupplier.PhoneNumber,
            Address = createdSupplier.Address,
            IsActive = createdSupplier.IsActive,
            CreateAt = createdSupplier.CreatedAt
        };

        return Result<SupplierDto>.Success(dto);
    }
}
