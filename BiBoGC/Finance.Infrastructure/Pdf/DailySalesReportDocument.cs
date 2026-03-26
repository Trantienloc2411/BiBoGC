using Finance.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Finance.Infrastructure.Pdf;

public class DailySalesReportDocument : IDocument
{
    private readonly DailySalesReportDto _report;

    public DailySalesReportDocument(DailySalesReportDto report) => _report = report;

    public DocumentMetadata GetMetadata() => new() { Title = $"Báo cáo doanh thu ngày {_report.Date:dd/MM/yyyy}" };
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
                col.Item().AlignCenter().Text("BÁO CÁO DOANH THU THEO NGÀY")
                    .Bold().FontSize(14);
                col.Item().AlignCenter()
                    .Text($"Ngày: {_report.Date:dd/MM/yyyy}")
                    .FontSize(11);
                col.Item().AlignRight()
                    .Text($"In lúc: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(8).FontColor(Colors.Grey.Darken2);
                col.Item().PaddingTop(4).LineHorizontal(1);
            });

            page.Content().PaddingTop(10).Column(col =>
            {
                // Summary
                col.Item().Text("TỔNG KẾT").Bold().FontSize(11);
                col.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                    });

                    AddSummaryCell(table, "Tổng doanh thu", _report.TotalRevenue.ToString("N0") + " đ", true);
                    AddSummaryCell(table, "Số giao dịch", _report.TransactionCount.ToString(), false);
                    AddSummaryCell(table, "Tăng trưởng (so hôm qua)",
                        _report.RevenueChangePercent.ToString("+0.##;-0.##;0") + "%", false);
                });

                // Top products
                col.Item().PaddingTop(12).Text("SẢN PHẨM BÁN CHẠY").Bold().FontSize(11);
                col.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(25);
                        c.RelativeColumn(3);
                        c.RelativeColumn(1);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                    });

                    AddTableHeader(table, "STT", "Sản phẩm", "SL", "Doanh thu", "");

                    var idx = 1;
                    foreach (var p in _report.TopSellingProducts)
                    {
                        table.Cell().Text(idx++.ToString());
                        table.Cell().Text($"{p.ProductName} - {p.VariantName}");
                        table.Cell().AlignCenter().Text(p.QuantitySold.ToString());
                        table.Cell().AlignRight().Text(p.Revenue.ToString("N0") + " đ");
                        table.Cell().Text("");
                    }
                });

                // Hourly breakdown
                col.Item().PaddingTop(12).Text("DOANH THU THEO GIỜ").Bold().FontSize(11);
                col.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(1);
                        c.RelativeColumn(2);
                        c.RelativeColumn(1);
                    });

                    AddTableHeader(table, "Giờ", "Doanh thu", "Giao dịch");

                    foreach (var h in _report.SalesByHour.OrderBy(x => x.Hour))
                    {
                        table.Cell().Text($"{h.Hour:00}:00");
                        table.Cell().AlignRight().Text(h.Revenue.ToString("N0") + " đ");
                        table.Cell().AlignCenter().Text(h.TransactionCount.ToString());
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

    private static void AddSummaryCell(TableDescriptor table, string label, string value, bool highlight)
    {
        table.Cell().Border(0.5f).Padding(6).Column(col =>
        {
            col.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken2);
            col.Item().Text(value).Bold().FontSize(highlight ? 13 : 10);
        });
    }

    private static void AddTableHeader(TableDescriptor table, params string[] headers)
    {
        table.Header(header =>
        {
            foreach (var h in headers)
                header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(h).Bold().FontSize(9);
        });
    }
}