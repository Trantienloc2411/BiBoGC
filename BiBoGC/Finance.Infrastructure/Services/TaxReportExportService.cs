using ClosedXML.Excel;
using Finance.Application.Interfaces;

namespace Finance.Infrastructure.Services;

public class TaxReportExportService : ITaxReportExportService
{
    public byte[] GenerateTaxReportExcel(
        int year,
        int? month,
        string storeTaxCode,
        List<InvoiceLineItem> invoices)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Bảng kê hoá đơn");

        // ── Title ──────────────────────────────────────────────────────────
        var periodLabel = month.HasValue
            ? $"Tháng {month:D2}/{year}"
            : $"Năm {year}";

        ws.Cell("A1").Value = "BẢNG KÊ HOÁ ĐƠN BÁN RA";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Range("A1:J1").Merge();

        ws.Cell("A2").Value = $"Kỳ kê khai: {periodLabel}";
        ws.Cell("A3").Value = $"Mã số thuế: {storeTaxCode}";
        ws.Cell("A4").Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";

        // ── Headers (row 6) ────────────────────────────────────────────────
        var headerRow = 6;
        var headers = new[]
        {
            "STT", "Số hoá đơn", "Ngày HĐ", "Tên khách hàng",
            "Doanh thu chưa thuế (đ)", "Giảm giá (đ)", "Thuế suất (%)",
            "Tiền thuế (đ)", "Tổng tiền thanh toán (đ)", "Ghi chú"
        };

        for (var col = 1; col <= headers.Length; col++)
        {
            var cell = ws.Cell(headerRow, col);
            cell.Value = headers[col - 1];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // ── Data rows ──────────────────────────────────────────────────────
        var dataStartRow = headerRow + 1;
        var idx = 1;

        foreach (var inv in invoices)
        {
            var row = dataStartRow + idx - 1;
            var taxRate = inv.SubTotal > 0
                ? Math.Round(inv.TaxAmount / inv.SubTotal * 100, 0)
                : 0;

            ws.Cell(row, 1).Value = idx++;
            ws.Cell(row, 2).Value = inv.InvoiceNumber;
            ws.Cell(row, 3).Value = inv.InvoiceDate.ToString("dd/MM/yyyy");
            ws.Cell(row, 4).Value = inv.CustomerName ?? "Khách lẻ";
            ws.Cell(row, 5).Value = inv.SubTotal;
            ws.Cell(row, 6).Value = inv.DiscountAmount;
            ws.Cell(row, 7).Value = (double)taxRate;
            ws.Cell(row, 8).Value = inv.TaxAmount;
            ws.Cell(row, 9).Value = inv.GrandTotal;
            ws.Cell(row, 10).Value = "";

            // Number format
            foreach (var numCol in new[] { 5, 6, 8, 9 })
                ws.Cell(row, numCol).Style.NumberFormat.Format = "#,##0";

            ws.Cell(row, 7).Style.NumberFormat.Format = "0";
        }

        // ── Totals row ─────────────────────────────────────────────────────
        var totalRow = dataStartRow + invoices.Count;
        ws.Cell(totalRow, 1).Value = "TỔNG CỘNG";
        ws.Cell(totalRow, 1).Style.Font.Bold = true;
        ws.Range(totalRow, 1, totalRow, 4).Merge();

        ws.Cell(totalRow, 5).FormulaA1 = $"=SUM(E{dataStartRow}:E{totalRow - 1})";
        ws.Cell(totalRow, 6).FormulaA1 = $"=SUM(F{dataStartRow}:F{totalRow - 1})";
        ws.Cell(totalRow, 8).FormulaA1 = $"=SUM(H{dataStartRow}:H{totalRow - 1})";
        ws.Cell(totalRow, 9).FormulaA1 = $"=SUM(I{dataStartRow}:I{totalRow - 1})";

        foreach (var numCol in new[] { 5, 6, 8, 9 })
        {
            ws.Cell(totalRow, numCol).Style.NumberFormat.Format = "#,##0";
            ws.Cell(totalRow, numCol).Style.Font.Bold = true;
        }

        // ── Borders for data range ─────────────────────────────────────────
        ws.Range(headerRow, 1, totalRow, headers.Length)
            .Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        ws.Range(headerRow, 1, totalRow, headers.Length)
            .Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

        // ── Column widths ──────────────────────────────────────────────────
        ws.Column(1).Width = 5;
        ws.Column(2).Width = 18;
        ws.Column(3).Width = 12;
        ws.Column(4).Width = 25;
        ws.Column(5).Width = 22;
        ws.Column(6).Width = 15;
        ws.Column(7).Width = 12;
        ws.Column(8).Width = 18;
        ws.Column(9).Width = 22;
        ws.Column(10).Width = 15;

        // ── Output ─────────────────────────────────────────────────────────
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}