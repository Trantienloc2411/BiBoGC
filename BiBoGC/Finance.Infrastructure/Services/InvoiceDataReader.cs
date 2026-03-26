using Finance.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Sale.Infrastructure.Data;

namespace Finance.Infrastructure.Services;

public class InvoiceDataReader : IInvoiceDataReader
{
    private readonly SaleDbContext _context;

    public InvoiceDataReader(SaleDbContext context)
    {
        _context = context;
    }

    public async Task<List<InvoiceLineItem>> GetInvoicesForTaxReportAsync(
        DateTime utcFrom, DateTime utcTo, CancellationToken ct = default)
    {
        var rows = await _context.Invoices
            .AsNoTracking()
            .Where(i => !i.IsDeleted && i.InvoiceDate >= utcFrom && i.InvoiceDate <= utcTo)
            .OrderBy(i => i.InvoiceDate)
            .Select(i => new
            {
                i.InvoiceNumber,
                i.InvoiceDate,
                i.CustomerName,
                i.StoreTaxCode,
                i.SubTotal,
                i.DiscountAmount,
                i.TaxAmount,
                i.GrandTotal
            })
            .ToListAsync(ct);

        return rows.Select(r => new InvoiceLineItem(
            r.InvoiceNumber,
            r.InvoiceDate,
            r.CustomerName,
            r.StoreTaxCode,
            r.SubTotal,
            r.DiscountAmount,
            r.TaxAmount,
            r.GrandTotal
        )).ToList();
    }
}