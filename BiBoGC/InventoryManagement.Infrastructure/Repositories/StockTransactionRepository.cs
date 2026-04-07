using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
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

    public async Task<(IEnumerable<StockTransaction>, int totalCount)> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? keyword = null,
        Guid? productId = null,
        StockTransactionType? transactionType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? sortBy = "TransactionDate",
        bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.StockTransactions
            .Where(st => !st.IsDeleted)
            .Include(st => st.Product)
            .Include(st => st.ProductBatch)
            .Include(st => st.Supplier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(st =>
                st.Product.Name.Contains(keyword) ||
                (st.Supplier != null && st.Supplier.Name.Contains(keyword)) ||
                (st.ProductBatch != null && st.ProductBatch.BatchNumber.Contains(keyword)));

        if (productId.HasValue)
            query = query.Where(st => st.ProductId == productId.Value);

        if (transactionType.HasValue)
            query = query.Where(st => st.TransactionType == transactionType.Value);

        if (fromDate.HasValue)
            query = query.Where(st => st.TransactionDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(st => st.TransactionDate <= toDate.Value);

        query = sortBy?.ToLower() switch
        {
            "quantity"        => sortDescending ? query.OrderByDescending(st => st.Quantity)        : query.OrderBy(st => st.Quantity),
            "unitprice"       => sortDescending ? query.OrderByDescending(st => st.UnitPrice)       : query.OrderBy(st => st.UnitPrice),
            "transactiontype" => sortDescending ? query.OrderByDescending(st => st.TransactionType) : query.OrderBy(st => st.TransactionType),
            _                 => sortDescending ? query.OrderByDescending(st => st.TransactionDate) : query.OrderBy(st => st.TransactionDate),
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var transactions = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (transactions, totalCount);
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