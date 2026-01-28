using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories;

public class StockTransactionRepository : IStockTransactionRepository
{
    private readonly InventoryDbContext _context;

    public StockTransactionRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<StockTransaction> AddAsync(StockTransaction stockTransaction,
        CancellationToken cancellationToken = default)
    {
        await _context.StockTransactions.AddAsync(stockTransaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return stockTransaction;
    }

    public async Task<(IEnumerable<StockTransaction>, int totalCount)> GetAllAsync(int pageNumber = 1,
        int pageSize = 10, string? keyword = null, CancellationToken cancellationToken = default)
    {
        var allTransactions = await _context.StockTransactions.Where(st => !st.IsDeleted)
            .Include(st => st.Product)
            .Include(st => st.ProductBatch)
            .Include(st => st.Supplier)
            .ToListAsync(cancellationToken);

        IEnumerable<StockTransaction> stockTransactions;

        if (keyword is null)
        {
            stockTransactions = allTransactions
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            var totalCount = allTransactions.Count;
            return (stockTransactions, totalCount);
        }
        else
        {
            stockTransactions = allTransactions
                .Where(st => st.Product.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                             (st?.Supplier?.ContactPerson != null &&
                              st.Supplier.ContactPerson.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                             (st?.ProductBatch?.BatchNumber != null &&
                              st.ProductBatch.BatchNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase)));

            var totalCount = stockTransactions.Count();

            var pagedTransactions = stockTransactions
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (pagedTransactions, totalCount);
        }
    }

    public async Task<StockTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StockTransactions
            .Include(st => st.Product)
            .Include(st => st.ProductBatch)
            .Include(st => st.Supplier)
            .FirstOrDefaultAsync(st => st.Id == id && !st.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<StockTransaction>> GetByProductIdAsync(Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StockTransactions
            .Where(st => st.ProductId == productId && !st.IsDeleted)
            .Include(st => st.Product)
            .Include(st => st.ProductBatch)
            .Include(st => st.Supplier)
            .AsAsyncEnumerable()
            .ToListAsync(cancellationToken);
    }
}