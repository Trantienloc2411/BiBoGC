namespace Finance.Application.Interfaces;

public interface ITaxReportExportService
{
    byte[] GenerateTaxReportExcel(
        int year,
        int? month,
        string storeTaxCode,
        List<InvoiceLineItem> invoices);
}