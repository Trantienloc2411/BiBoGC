using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using MediatR;
using Shared.Application.Common;


namespace InventoryManagement.Application.Commands.UpdateSupplier
{
    public class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, Result<SupplierDto>>
    {
        private readonly ISupplierRepository _supplierRepository;
        public UpdateSupplierCommandHandler(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }
        public async Task<Result<SupplierDto>> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
        {
            var supplierToUpdate = await _supplierRepository.GetByIdAsync(request.Id); 
            if(supplierToUpdate is null)
            {
                return Result<SupplierDto>.Failure($"Supplier với mã {request.Id} không tìm thấy.");
            }
            else
            {
                supplierToUpdate.UpdateInfo(request.Name, request.ContactName, request.ContactPhone, request.Address);
                if(request.IsActive)
                {
                    supplierToUpdate.Activate();
                }
                else
                {
                    supplierToUpdate.Deactivate();
                }
                supplierToUpdate.UpdatedAt = DateTime.UtcNow;
                
                await _supplierRepository.UpdateAsync(supplierToUpdate);
                var supplierDto = new SupplierDto
                {
                    Id = supplierToUpdate.Id,
                    Name = supplierToUpdate.Name,
                    ContactName = supplierToUpdate.ContactPerson,
                    ContactPhone = supplierToUpdate.PhoneNumber,
                    Address = supplierToUpdate.Address,
                    IsActive = supplierToUpdate.IsActive,
                    CreateAt = supplierToUpdate.CreatedAt,
                    Transactions = null // Assuming transactions are not updated here
                    
                };
                return Result<SupplierDto>.Success(supplierDto);
            }
        }
    }
}
