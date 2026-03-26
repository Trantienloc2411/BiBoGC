using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sale.Application.DTOs;

namespace Sale.Infrastructure.Pdf;

public class InvoicePdfDocument : IDocument
{
    private readonly InvoiceDto _invoice;

    public InvoicePdfDocument(InvoiceDto invoice)
    {
        _invoice = invoice;
    }

    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"Hóa đơn {_invoice.InvoiceNumber}",
        Author = _invoice.StoreName
    };

    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A5);
            page.Margin(20);
            page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

            page.Content().Column(col =>
            {
                // ── Store header ──────────────────────────────────────────
                col.Item().AlignCenter().Text(_invoice.StoreName)
                    .Bold().FontSize(14);

                col.Item().AlignCenter().Text(_invoice.StoreAddress)
                    .FontSize(8).FontColor(Colors.Grey.Darken2);

                col.Item().AlignCenter().Text($"ĐT: {_invoice.StorePhone}")
                    .FontSize(8).FontColor(Colors.Grey.Darken2);

                if (!string.IsNullOrWhiteSpace(_invoice.StoreTaxCode))
                    col.Item().AlignCenter().Text($"MST: {_invoice.StoreTaxCode}")
                        .FontSize(8).FontColor(Colors.Grey.Darken2);

                col.Item().PaddingVertical(4).LineHorizontal(0.5f);

                // ── Invoice title ─────────────────────────────────────────
                col.Item().AlignCenter().Text("HÓA ĐƠN BÁN HÀNG")
                    .Bold().FontSize(12);

                col.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Số HĐ: {_invoice.InvoiceNumber}").FontSize(8);
                    row.RelativeItem().AlignRight()
                        .Text($"Ngày: {_invoice.InvoiceDate:dd/MM/yyyy HH:mm}").FontSize(8);
                });

                col.Item().Row(row => { row.RelativeItem().Text($"Đơn hàng: {_invoice.OrderNumber}").FontSize(8); });

                // ── Customer info ─────────────────────────────────────────
                if (!string.IsNullOrWhiteSpace(_invoice.CustomerName))
                    col.Item().Text($"Khách: {_invoice.CustomerName}  |  {_invoice.CustomerPhone}").FontSize(8);

                col.Item().PaddingVertical(4).LineHorizontal(0.5f);

                // ── Items table ───────────────────────────────────────────
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(20); // STT
                        cols.RelativeColumn(3); // Tên hàng
                        cols.RelativeColumn(1); // SL
                        cols.RelativeColumn(2); // Đơn giá
                        cols.RelativeColumn(2); // Thành tiền
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Text("STT").Bold().FontSize(8);
                        header.Cell().Text("Tên hàng").Bold().FontSize(8);
                        header.Cell().AlignCenter().Text("SL").Bold().FontSize(8);
                        header.Cell().AlignRight().Text("Đơn giá").Bold().FontSize(8);
                        header.Cell().AlignRight().Text("T.Tiền").Bold().FontSize(8);
                    });

                    var index = 1;
                    foreach (var item in _invoice.Items)
                    {
                        table.Cell().Text(index++.ToString()).FontSize(8);
                        table.Cell().Text($"{item.ProductName}\n{item.VariantName}").FontSize(8);
                        table.Cell().AlignCenter().Text(item.Quantity.ToString()).FontSize(8);
                        table.Cell().AlignRight().Text(item.UnitPrice.ToString("N0")).FontSize(8);
                        table.Cell().AlignRight().Text(item.LineTotal.ToString("N0")).FontSize(8);
                    }
                });

                col.Item().PaddingVertical(2).LineHorizontal(0.5f);

                // ── Totals ────────────────────────────────────────────────
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("Tạm tính:").FontSize(8);
                    row.ConstantItem(80).AlignRight()
                        .Text(_invoice.SubTotal.ToString("N0") + " đ").FontSize(8);
                });

                if (_invoice.DiscountAmount > 0)
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Giảm giá:").FontSize(8).FontColor(Colors.Red.Medium);
                        row.ConstantItem(80).AlignRight()
                            .Text("-" + _invoice.DiscountAmount.ToString("N0") + " đ")
                            .FontSize(8).FontColor(Colors.Red.Medium);
                    });

                if (_invoice.TaxAmount > 0)
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Thuế VAT:").FontSize(8);
                        row.ConstantItem(80).AlignRight()
                            .Text(_invoice.TaxAmount.ToString("N0") + " đ").FontSize(8);
                    });

                col.Item().PaddingVertical(2).LineHorizontal(0.5f);

                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("TỔNG CỘNG:").Bold().FontSize(10);
                    row.ConstantItem(80).AlignRight()
                        .Text(_invoice.GrandTotal.ToString("N0") + " đ").Bold().FontSize(10);
                });

                col.Item().Row(row =>
                {
                    row.RelativeItem().Text($"TT ({_invoice.PaymentMethod}):").FontSize(8);
                    row.ConstantItem(80).AlignRight()
                        .Text(_invoice.AmountPaid.ToString("N0") + " đ").FontSize(8);
                });

                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("Tiền thừa:").FontSize(8);
                    row.ConstantItem(80).AlignRight()
                        .Text(_invoice.ChangeAmount.ToString("N0") + " đ").FontSize(8);
                });

                col.Item().PaddingVertical(6).LineHorizontal(0.5f);

                col.Item().AlignCenter().Text("Cảm ơn quý khách!")
                    .Italic().FontSize(8).FontColor(Colors.Grey.Darken2);

                col.Item().AlignCenter()
                    .Text($"In lúc: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                    .FontSize(7).FontColor(Colors.Grey.Medium);
            });
        });
    }
}