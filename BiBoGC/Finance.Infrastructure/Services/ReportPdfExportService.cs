using Finance.Application.DTOs;
using Finance.Application.Interfaces;
using Finance.Infrastructure.Pdf;
using QuestPDF.Fluent;

namespace Finance.Infrastructure.Services;

public class ReportPdfExportService : IReportPdfExportService
{
    public byte[] GenerateDailySalesReportPdf(DailySalesReportDto report)
        => new DailySalesReportDocument(report).GeneratePdf();

    public byte[] GenerateMonthlySalesReportPdf(MonthlySalesReportDto report)
        => new MonthlySalesReportDocument(report).GeneratePdf();

    public byte[] GenerateFinancialReportPdf(MonthlyFinancialReportDto report)
        => new FinancialReportDocument(report).GeneratePdf();
}