using Finance.Application.Interfaces;
using Finance.Application.ReadModels;
using Microsoft.EntityFrameworkCore;
using Sale.Domain.Enum;
using Sale.Infrastructure.Data;

namespace Finance.Infrastructure.Services;

public class SalesDataReader : ISalesDataReader
{
    private readonly SaleDbContext _context;

    public SalesDataReader(SaleDbContext context)
    {
        _context = context;
    }

    public async Task<SalesAggregate> GetAggregateAsync(
        DateTime utcFrom, DateTime utcTo, CancellationToken ct = default)
    {
        var result = await _context.SalesOrders
            .Where(o => o.Status == OrderStatus.Completed
                        && o.OrderDate >= utcFrom
                        && o.OrderDate <= utcTo)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalRevenue = g.Sum(o => o.TotalAmount),
                TransactionCount = g.Count()
            })
            .FirstOrDefaultAsync(ct);

        return result is null
            ? new SalesAggregate(0, 0)
            : new SalesAggregate(result.TotalRevenue, result.TransactionCount);
    }

    public async Task<List<TopProduct>> GetTopProductsAsync(
        DateTime utcFrom, DateTime utcTo, int take, CancellationToken ct = default)
    {
        // Step 1: get qualifying order IDs (avoids navigation-property join inside GroupBy)
        var orderIds = await _context.SalesOrders
            .Where(o => o.Status == OrderStatus.Completed
                        && o.OrderDate >= utcFrom
                        && o.OrderDate <= utcTo)
            .Select(o => o.Id)
            .ToListAsync(ct);

        // Step 2: project into anonymous type — EF Core can translate this
        // then map to record in memory (record constructors inside GroupBy.Select cannot be translated)
        var rows = await _context.SalesOrderItems
            .Where(i => orderIds.Contains(i.SalesOrderId))
            .GroupBy(i => new { i.ProductName, i.VariantName })
            .Select(g => new
            {
                g.Key.ProductName,
                g.Key.VariantName,
                QuantitySold = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.UnitPrice * (decimal)i.Quantity)
            })
            .OrderByDescending(r => r.Revenue)
            .Take(take)
            .ToListAsync(ct);

        return rows.Select(r => new TopProduct(r.ProductName, r.VariantName, r.QuantitySold, r.Revenue))
            .ToList();
    }

    public async Task<List<HourlySales>> GetHourlySalesAsync(
        DateTime utcFrom, DateTime utcTo, CancellationToken ct = default)
    {
        var rows = await _context.SalesOrders
            .Where(o => o.Status == OrderStatus.Completed
                        && o.OrderDate >= utcFrom
                        && o.OrderDate <= utcTo)
            .GroupBy(o => o.OrderDate.Hour)
            .Select(g => new
            {
                Hour = g.Key,
                Revenue = g.Sum(o => o.TotalAmount),
                TransactionCount = g.Count()
            })
            .OrderBy(r => r.Hour)
            .ToListAsync(ct);

        return rows.Select(r => new HourlySales(r.Hour, r.Revenue, r.TransactionCount))
            .ToList();
    }

    public async Task<List<DailySales>> GetDailyBreakdownAsync(
        DateTime utcFrom, DateTime utcTo, CancellationToken ct = default)
    {
        var rows = await _context.SalesOrders
            .Where(o => o.Status == OrderStatus.Completed
                        && o.OrderDate >= utcFrom
                        && o.OrderDate <= utcTo)
            .GroupBy(o => o.OrderDate.Day)
            .Select(g => new
            {
                Day = g.Key,
                Revenue = g.Sum(o => o.TotalAmount),
                TransactionCount = g.Count()
            })
            .OrderBy(r => r.Day)
            .ToListAsync(ct);

        return rows.Select(r => new DailySales(r.Day, r.Revenue, r.TransactionCount))
            .ToList();
    }

    public async Task<List<MonthlySales>> GetMonthlyBreakdownAsync(int year, CancellationToken ct = default)
    {
        var utcFrom = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var utcTo = new DateTime(year, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc);

        var rows = await _context.SalesOrders
            .Where(o => o.Status == OrderStatus.Completed
                        && o.OrderDate >= utcFrom
                        && o.OrderDate <= utcTo)
            .GroupBy(o => o.OrderDate.Month)
            .Select(g => new
            {
                Month = g.Key,
                Revenue = g.Sum(o => o.TotalAmount),
                TransactionCount = g.Count()
            })
            .OrderBy(r => r.Month)
            .ToListAsync(ct);

        return rows.Select(r => new MonthlySales(r.Month, r.Revenue, r.TransactionCount))
            .ToList();
    }
}