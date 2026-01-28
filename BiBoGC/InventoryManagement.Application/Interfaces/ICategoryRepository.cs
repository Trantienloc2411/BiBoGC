using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Category?> GetByIdWithSubCategoriesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Category>> GetAllAsync(bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(string name, Guid? excludeCategoryId = null,
        CancellationToken cancellationToken = default);

    Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default);
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
    Task DeleteAsync(Category category, CancellationToken cancellationToken = default);
    Task<int> GetProductCountAsync(Guid categoryId, CancellationToken cancellationToken = default);
}