using Finance.Application.DTOs;

namespace Finance.Application.Interfaces;

public interface IReportPdfExportService
{
    byte[] GenerateDailySalesReportPdf(DailySalesReportDto report);
    byte[] GenerateMonthlySalesReportPdf(MonthlySalesReportDto report);
    byte[] GenerateFinancialReportPdf(MonthlyFinancialReportDto report);
}