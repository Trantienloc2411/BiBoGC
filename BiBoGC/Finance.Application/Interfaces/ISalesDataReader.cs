using Finance.Application.ReadModels;

namespace Finance.Application.Interfaces;

/// <summary>
/// Read-only abstraction over SaleDbContext for reporting queries.
/// Implemented in Finance.Infrastructure — Finance.Application stays free of EF Core.
/// </summary>
public interface ISalesDataReader
{
    Task<SalesAggregate> GetAggregateAsync(
        DateTime utcFrom, DateTime utcTo, CancellationToken ct = default);

    Task<List<TopProduct>> GetTopProductsAsync(
        DateTime utcFrom, DateTime utcTo, int take, CancellationToken ct = default);

    Task<List<HourlySales>> GetHourlySalesAsync(
        DateTime utcFrom, DateTime utcTo, CancellationToken ct = default);

    Task<List<DailySales>> GetDailyBreakdownAsync(
        DateTime utcFrom, DateTime utcTo, CancellationToken ct = default);

    Task<List<MonthlySales>> GetMonthlyBreakdownAsync(int year, CancellationToken ct = default);
}