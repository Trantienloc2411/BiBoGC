using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Interfaces;

public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    //TO-DO: Improvement - Merge GetAllAsync and SearchAsync methods to optimize data retrieval
    Task<(IEnumerable<Supplier> Items, int TotalCount)> GetAllAsync(int pageSize = 10, int pageNumber = 1,
        string? searchTerm = null,
        bool? isActive = null,
        string? sortBy = "Name",
        bool sortDescending = false,
        CancellationToken cancellationToken = default);

    //Task<IEnumerable<Supplier>> SearchAsync(string? searchTerm, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeSupplierId = null,
        CancellationToken cancellationToken = default);

    Task<Supplier> AddAsync(Supplier supplier, CancellationToken cancellationToken = default);
    Task UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default);
    Task DeleteAsync(Supplier supplier, CancellationToken cancellationToken = default);
}