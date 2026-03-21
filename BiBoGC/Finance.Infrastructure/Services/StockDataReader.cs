using Finance.Application.Interfaces;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Services;

public class StockDataReader : IStockDataReader
{
    private readonly InventoryDbContext _context;

    public StockDataReader(InventoryDbContext context)
    {
        _context = context;
    }

    public Task<decimal> GetCogsAsync(
        DateTime utcFrom, DateTime utcTo, CancellationToken ct = default)
    {
        return _context.StockTransactions
            .Where(t => t.TransactionType == StockTransactionType.Sale
                        && t.TransactionDate >= utcFrom
                        && t.TransactionDate <= utcTo)
            .SumAsync(t => t.UnitPrice * t.Quantity, ct);
    }
}
