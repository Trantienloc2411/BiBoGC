namespace Finance.Application.Interfaces;

/// <summary>
/// Read-only abstraction over InventoryDbContext for COGS queries.
/// </summary>
public interface IStockDataReader
{
    Task<decimal> GetCogsAsync(
        DateTime utcFrom, DateTime utcTo, CancellationToken ct = default);
}
