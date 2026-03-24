using Sale.Domain.Domain;
using Sale.Domain.Enum;

namespace Sale.Application.Interfaces;

public interface ISalesOrderRepository
{
    Task<SalesOrder?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SalesOrder?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<SalesOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);

    Task<(IEnumerable<SalesOrder> Items, int TotalCount, IReadOnlyDictionary<Guid, string> InvoiceNumbers)> GetAllAsync(
        int page,
        int pageSize,
        OrderStatus? status = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string? search = null,
        CancellationToken ct = default);

    Task<SalesOrder> AddAsync(SalesOrder order, CancellationToken ct = default);
    Task UpdateAsync(SalesOrder order, CancellationToken ct = default);
    Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken ct = default);
}