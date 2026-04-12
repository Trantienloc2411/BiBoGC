using Microsoft.EntityFrameworkCore;
using Sale.Application.Interfaces;
using Sale.Domain.Domain;
using Sale.Infrastructure.Data;

namespace Sale.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly SaleDbContext _context;

    public InvoiceRepository(SaleDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Invoices
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<Invoice?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Invoices
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<Invoice?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _context.Invoices
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.SalesOrderId == orderId, ct);
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default)
    {
        return await _context.Invoices
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.InvoiceNumber == invoiceNumber, ct);
    }

    public async Task<(IEnumerable<Invoice> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken ct = default)
    {
        var query = _context.Invoices
            .Include(x => x.Items)
            .AsQueryable();

        if (dateFrom.HasValue)
            query = query.Where(x => x.InvoiceDate >= DateTime.SpecifyKind(dateFrom.Value, DateTimeKind.Utc));

        if (dateTo.HasValue)
            query = query.Where(x => x.InvoiceDate <= DateTime.SpecifyKind(dateTo.Value, DateTimeKind.Utc));

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IEnumerable<Invoice>> GetAllForExportAsync(
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken ct = default)
    {
        var query = _context.Invoices
            .Include(x => x.Items)
            .AsQueryable();

        if (dateFrom.HasValue)
            query = query.Where(x => x.InvoiceDate >= DateTime.SpecifyKind(dateFrom.Value, DateTimeKind.Utc));

        if (dateTo.HasValue)
            query = query.Where(x => x.InvoiceDate <= DateTime.SpecifyKind(dateTo.Value, DateTimeKind.Utc));

        return await query
            .OrderBy(x => x.InvoiceDate)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<Invoice> AddAsync(Invoice invoice, CancellationToken ct = default)
    {
        await _context.Invoices.AddAsync(invoice, ct);
        await _context.SaveChangesAsync(ct);
        return invoice;
    }
}