using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using MediatR;
using Shared.Application.Common;


namespace InventoryManagement.Application.Queries.GetSupplier
{
    public class GetSupplierQueryHandler : IRequestHandler<GetSupplierQuery, Result<SupplierDto>>
    {
        private readonly ISupplierRepository _supplierRepository;
        public GetSupplierQueryHandler(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }
        public async Task<Result<SupplierDto>> Handle
            (GetSupplierQuery request, CancellationToken cancellationToken)
        {
            var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId);

            if (supplier is null)
            {
                return Result<SupplierDto>.Failure($"Supplier with ID {request.SupplierId} not found.");
            }
            else
            {
                var supplierDto = new SupplierDto
                {
                    Id = supplier.Id,
                    Name = supplier.Name,
                    ContactName = supplier.ContactPerson,
                    ContactPhone = supplier.PhoneNumber,
                    Address = supplier.Address,
                    IsActive = supplier.IsActive,
                    CreateAt = supplier.CreatedAt,
                    Transactions = supplier.Transactions.Select(t => new StockTransactionDto
                    {
                        Id = t.Id,
                        ProductName = t.Product.Name,
                        BatchNumber = t.ProductBatch != null ? t.ProductBatch.BatchNumber : null,
                        TransactionType = t.TransactionType.ToString(),
                        TransactionDate = t.TransactionDate,
                    })
                };
                return Result<SupplierDto>.Success(supplierDto);
            }
        }
    }
}
