using Shared.Application.Common;

namespace Finance.Application.Interfaces;

public interface ITaxReportExportService
{
    /// <summary>
    /// Fills the S2a-HKD.xlsx template with invoice data.
    /// Returns a single Excel file or a ZIP when data exceeds the template capacity (20 rows).
    /// </summary>
    ExportFileResult GenerateTaxReportExcel(
        int fromMonth, int fromYear,
        int toMonth, int toYear,
        List<InvoiceLineItem> invoices);
}
