using InventoryManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryManagement.Application.Interfaces
{
    public interface ISupplerRepository
    {
        Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Supplier>> GetAllAsync(int pageSize = 10, int pageNumber = 1, CancellationToken cancellationToken = default);
        Task<IEnumerable<Supplier>> SearchAsync(string? searchTerm, CancellationToken cancellationToken = default);
        Task<bool> NameExistsAsync(string name, Guid? excludeSupplierId = null, CancellationToken cancellationToken = default);
        Task<Supplier> AddAsync(Supplier supplier, CancellationToken cancellationToken = default);
        Task UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default);
        Task DeleteAsync(Supplier supplier, CancellationToken cancellationToken = default);
    }
}
