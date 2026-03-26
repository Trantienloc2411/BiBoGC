namespace Finance.Application.Interfaces;

public record InvoiceLineItem(
    string InvoiceNumber,
    DateTime InvoiceDate,
    string? CustomerName,
    string? StoreTaxCode,
    decimal SubTotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal GrandTotal);

public interface IInvoiceDataReader
{
    Task<List<InvoiceLineItem>> GetInvoicesForTaxReportAsync(
        DateTime utcFrom, DateTime utcTo, CancellationToken ct = default);
}