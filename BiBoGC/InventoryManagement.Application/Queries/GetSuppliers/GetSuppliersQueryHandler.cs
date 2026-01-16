using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using MediatR;
using Shared.Application.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryManagement.Application.Queries.GetSuppliers
{
    public class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, PaginatedResult<SuppliersDto>>
    {
        private readonly ISupplierRepository _supplierRepository; 
        public GetSuppliersQueryHandler(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }   
        public async Task<PaginatedResult<SuppliersDto>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
        {
            var (suppliers, totalCount) = await _supplierRepository.GetAllAsync(
                    pageNumber: request.PageNumber,
                    pageSize: request.PageSize,
                    searchTerm: request.SearchTerm,
                    isActive: request.IsActive,
                    sortBy: request.SortBy,
                    sortDescending: request.SortDescending
                    );
                
            var dtos = suppliers.Select(s => new SuppliersDto
            {
                Id = s.Id,
                Name = s.Name,
                ContactName = s.ContactPerson,
                ContactPhone = s.PhoneNumber,
                Address = s.Address,
                IsActive = s.IsActive,
                CreateAt = s.CreatedAt
            });
            return new PaginatedResult<SuppliersDto>
            {
                Items = dtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
