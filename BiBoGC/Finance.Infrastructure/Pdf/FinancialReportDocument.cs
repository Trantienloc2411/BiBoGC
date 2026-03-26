using Finance.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Finance.Infrastructure.Pdf;

public class FinancialReportDocument : IDocument
{
    private readonly MonthlyFinancialReportDto _report;

    public FinancialReportDocument(MonthlyFinancialReportDto report) => _report = report;

    public DocumentMetadata GetMetadata() => new()
        { Title = $"Báo cáo tài chính tháng {_report.Month}/{_report.Year}" };

    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(40);
            page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

            page.Header().Column(col =>
            {
                col.Item().AlignCenter().Text("BÁO CÁO TÀI CHÍNH THÁNG")
                    .Bold().FontSize(14);
                col.Item().AlignCenter()
                    .Text($"Tháng {_report.Month:D2}/{_report.Year}")
                    .FontSize(11);
                col.Item().AlignRight()
                    .Text($"In lúc: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(8).FontColor(Colors.Grey.Darken2);
                col.Item().PaddingTop(4).LineHorizontal(1);
            });

            page.Content().PaddingTop(16).Column(col =>
            {
                col.Item().Text("KẾT QUẢ KINH DOANH (P&L)").Bold().FontSize(12);
                col.Item().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(2);
                    });

                    AddPnlRow(table, "Doanh thu thuần", _report.TotalRevenue, false, false);
                    AddPnlRow(table, "Giá vốn hàng bán (COGS)", _report.TotalCogs, true, false);
                    AddPnlRow(table, "Lợi nhuận gộp", _report.GrossProfit, false, true);
                    AddPnlRow(table, "Chi phí hoạt động", _report.TotalExpenses, true, false);

                    // Net profit row with color
                    var isProfit = _report.NetProfit >= 0;
                    table.Cell().Background(isProfit ? Colors.Green.Lighten4 : Colors.Red.Lighten4)
                        .Padding(6).Text("LỢI NHUẬN RÒNG").Bold();
                    table.Cell().Background(isProfit ? Colors.Green.Lighten4 : Colors.Red.Lighten4)
                        .Padding(6).AlignRight()
                        .Text(_report.NetProfit.ToString("N0") + " đ")
                        .Bold()
                        .FontColor(isProfit ? Colors.Green.Darken3 : Colors.Red.Darken3);
                });

                col.Item().PaddingTop(12).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(2);
                    });

                    table.Cell().Padding(4).Text("Tỷ suất lợi nhuận (%):").Bold();
                    table.Cell().Padding(4).AlignRight()
                        .Text(_report.ProfitMarginPercent.ToString("N2") + " %").Bold();
                });
            });

            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("Trang ").FontSize(8);
                text.CurrentPageNumber().FontSize(8);
                text.Span(" / ").FontSize(8);
                text.TotalPages().FontSize(8);
            });
        });
    }

    private static void AddPnlRow(TableDescriptor table, string label, decimal value,
        bool isDeduction, bool isSubtotal)
    {
        var bg = isSubtotal ? Colors.Grey.Lighten3 : Colors.White;
        var labelStyle = TextStyle.Default;
        if (isSubtotal) labelStyle = labelStyle.Bold();
        if (isDeduction) labelStyle = labelStyle.Italic();

        var valueStyle = TextStyle.Default;
        if (isSubtotal) valueStyle = valueStyle.Bold();

        table.Cell().Background(bg).Padding(6).Text(label).Style(labelStyle);
        table.Cell().Background(bg).Padding(6).AlignRight()
            .Text((isDeduction ? "-" : "") + value.ToString("N0") + " đ").Style(valueStyle);
    }
}