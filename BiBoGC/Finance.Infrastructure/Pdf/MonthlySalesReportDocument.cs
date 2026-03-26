using Finance.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Finance.Infrastructure.Pdf;

public class MonthlySalesReportDocument : IDocument
{
    private readonly MonthlySalesReportDto _report;

    public MonthlySalesReportDocument(MonthlySalesReportDto report) => _report = report;

    public DocumentMetadata GetMetadata() => new()
        { Title = $"Báo cáo doanh thu tháng {_report.Month}/{_report.Year}" };

    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

            page.Header().Column(col =>
            {
                col.Item().AlignCenter().Text("BÁO CÁO DOANH THU THEO THÁNG")
                    .Bold().FontSize(14);
                col.Item().AlignCenter()
                    .Text($"Tháng {_report.Month:D2}/{_report.Year}")
                    .FontSize(11);
                col.Item().AlignRight()
                    .Text($"In lúc: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(8).FontColor(Colors.Grey.Darken2);
                col.Item().PaddingTop(4).LineHorizontal(1);
            });

            page.Content().PaddingTop(10).Column(col =>
            {
                // Summary row
                col.Item().Text("TỔNG KẾT").Bold().FontSize(11);
                col.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                    });

                    AddSummaryCell(table, "Tổng doanh thu", _report.TotalRevenue.ToString("N0") + " đ");
                    AddSummaryCell(table, "Số giao dịch", _report.TransactionCount.ToString());
                    AddSummaryCell(table, "Tăng trưởng MoM",
                        _report.PreviousMonthChangePercent.ToString("+0.##;-0.##;0") + "%");
                    AddSummaryCell(table, "Tăng trưởng YoY",
                        _report.YoYChangePercent.ToString("+0.##;-0.##;0") + "%");
                });

                // Daily breakdown
                col.Item().PaddingTop(12).Text("DOANH THU THEO NGÀY").Bold().FontSize(11);
                col.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(1);
                        c.RelativeColumn(2);
                        c.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Ngày").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Doanh thu").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Giao dịch").Bold().FontSize(9);
                    });

                    foreach (var d in _report.DailyBreakdown.OrderBy(x => x.Day))
                    {
                        table.Cell().Padding(3).Text($"{d.Day:D2}/{_report.Month:D2}/{_report.Year}");
                        table.Cell().Padding(3).AlignRight().Text(d.Revenue.ToString("N0") + " đ");
                        table.Cell().Padding(3).AlignCenter().Text(d.TransactionCount.ToString());
                    }
                });

                // Top products
                col.Item().PaddingTop(12).Text("SẢN PHẨM BÁN CHẠY (TOP 10)").Bold().FontSize(11);
                col.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(25);
                        c.RelativeColumn(3);
                        c.RelativeColumn(1);
                        c.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("STT").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Sản phẩm").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("SL").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Doanh thu").Bold().FontSize(9);
                    });

                    var idx = 1;
                    foreach (var p in _report.TopSellingProducts)
                    {
                        table.Cell().Padding(3).Text(idx++.ToString());
                        table.Cell().Padding(3).Text($"{p.ProductName} - {p.VariantName}");
                        table.Cell().Padding(3).AlignCenter().Text(p.QuantitySold.ToString());
                        table.Cell().Padding(3).AlignRight().Text(p.Revenue.ToString("N0") + " đ");
                    }
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

    private static void AddSummaryCell(TableDescriptor table, string label, string value)
    {
        table.Cell().Border(0.5f).Padding(6).Column(col =>
        {
            col.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken2);
            col.Item().Text(value).Bold().FontSize(11);
        });
    }
}