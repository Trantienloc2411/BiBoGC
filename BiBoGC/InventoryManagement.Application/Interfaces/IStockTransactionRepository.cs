using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Application.Interfaces;

public interface IStockTransactionRepository
{
    Task<StockTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<StockTransaction>> GetByProductIdAsync(Guid productId,
        CancellationToken cancellationToken = default);

    Task<(IEnumerable<StockTransaction>, int totalCount)> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? keyword = null,
        Guid? productId = null,
        StockTransactionType? transactionType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? sortBy = "TransactionDate",
        bool sortDescending = true,
        CancellationToken cancellationToken = default);

    Task<StockTransaction> AddAsync(StockTransaction stockTransaction, CancellationToken cancellationToken = default);
}