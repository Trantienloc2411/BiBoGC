using Sale.Domain.Domain;

namespace Sale.Application.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Invoice?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<Invoice?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default);

    Task<(IEnumerable<Invoice> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken ct = default);

    Task<IEnumerable<Invoice>> GetAllForExportAsync(
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken ct = default);

    Task<Invoice> AddAsync(Invoice invoice, CancellationToken ct = default);
}