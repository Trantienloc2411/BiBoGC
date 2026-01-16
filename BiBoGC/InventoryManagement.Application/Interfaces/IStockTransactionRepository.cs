using InventoryManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryManagement.Application.Interfaces
{
    public interface IStockTransactionRepository
    {
        Task<StockTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<StockTransaction>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<StockTransaction>, int totalCount)> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? keyword = null,  CancellationToken cancellationToken = default);
        Task<StockTransaction> AddAsync(StockTransaction stockTransaction, CancellationToken cancellationToken = default);

    }
}
