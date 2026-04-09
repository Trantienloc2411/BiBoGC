using ClosedXML.Excel;
using Finance.Application.Interfaces;
using Shared.Application.Common;
using System.IO.Compression;
using System.Reflection;

namespace Finance.Infrastructure.Services;

/// <summary>
/// Fills the S2a-HKD.xlsx template (Sổ chi tiết doanh thu) with invoice data.
/// Template capacity: 20 data rows (rows 12–31). Splits into multiple files when exceeded.
/// </summary>
public class TaxReportExportService : ITaxReportExportService
{
    private const int DataStartRow = 12;
    private const int DataEndRow = 31;
    private const int MaxRowsPerFile = DataEndRow - DataStartRow + 1; // 20

    private const int TotalRow = 32;
    private const int GtgtRow = 33;
    private const int TncnRow = 34;
    private const int TotalGtgtDueRow = 35;
    private const int TotalTncnDueRow = 36;
    private const int DateRow = 37;

    private const decimal GtgtRate = 0.01m;  // 1% per TT 40/2021/TT-BTC
    private const decimal TncnRate = 0.005m; // 0.5% per TT 40/2021/TT-BTC

    private static readonly string TemplateResourceName =
        "Finance.Infrastructure.Templates.S2a-HKD.xlsx";

    public ExportFileResult GenerateTaxReportExcel(
        int fromMonth, int fromYear,
        int toMonth, int toYear,
        List<InvoiceLineItem> invoices)
    {
        var chunks = invoices
            .Select((item, i) => (item, i))
            .GroupBy(x => x.i / MaxRowsPerFile)
            .Select(g => g.Select(x => x.item).ToList())
            .ToList();

        if (chunks.Count == 1)
        {
            var data = GenerateSingleFile(chunks[0], fromMonth, fromYear, toMonth, toYear, fileIndex: 1);
            return new ExportFileResult(data, IsZip: false);
        }

        // Multiple files → ZIP
        using var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                var fileBytes = GenerateSingleFile(
                    chunks[i], fromMonth, fromYear, toMonth, toYear, fileIndex: i + 1);

                var entryName = $"S2a-HKD_{fromYear}_T{fromMonth:D2}-T{toMonth:D2}_part{i + 1}.xlsx";
                var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                using var entryStream = entry.Open();
                entryStream.Write(fileBytes, 0, fileBytes.Length);
            }
        }

        return new ExportFileResult(zipStream.ToArray(), IsZip: true);
    }

    private byte[] GenerateSingleFile(
        List<InvoiceLineItem> chunk,
        int fromMonth, int fromYear,
        int toMonth, int toYear,
        int fileIndex)
    {
        using var templateStream = LoadTemplate();
        using var workbook = new XLWorkbook(templateStream);
        var ws = workbook.Worksheet(1);

        // Replace period placeholders in A6
        ReplaceCellPlaceholders(ws.Cell("A6"), new Dictionary<string, string>
        {
            ["{{FROM MONTH}}"] = fromMonth.ToString("D2"),
            ["{{FROM YEAR}}"]  = fromYear.ToString(),
            ["{{TO MONTH}}"]   = toMonth.ToString("D2"),
            ["{{TO YEAR}}"]    = toYear.ToString()
        });

        // Fill data rows
        decimal totalRevenue = 0;
        decimal totalTax = 0;

        for (int i = 0; i < chunk.Count; i++)
        {
            var inv = chunk[i];
            int row = DataStartRow + i;

            ws.Cell(row, 1).Value = inv.InvoiceNumber;
            ws.Cell(row, 2).Value = inv.InvoiceDate.ToLocalTime().ToString("dd/MM/yyyy");
            ws.Cell(row, 3).Value = inv.CustomerName ?? "Khách lẻ";
            ws.Cell(row, 4).Value = (double)inv.GrandTotal;

            totalRevenue += inv.GrandTotal;
            totalTax     += inv.TaxAmount;
        }

        // Summary rows
        ws.Cell(TotalRow, 4).Value     = (double)totalRevenue;
        ws.Cell(GtgtRow, 4).Value      = (double)totalTax;
        ws.Cell(TncnRow, 4).Value      = 0;
        ws.Cell(TotalGtgtDueRow, 4).Value = (double)Math.Round(totalRevenue * GtgtRate, 0);
        ws.Cell(TotalTncnDueRow, 4).Value = (double)Math.Round(totalRevenue * TncnRate, 0);

        // Replace date placeholders in D37
        var now = DateTime.Now;
        ReplaceCellPlaceholders(ws.Cell(DateRow, 4), new Dictionary<string, string>
        {
            ["{{DATECURRENT}}"]  = now.Day.ToString(),
            ["{{MONTHCURRENT}}"] = now.Month.ToString(),
            ["{{YEARCURRENT}}"]  = now.Year.ToString()
        });

        using var output = new MemoryStream();
        workbook.SaveAs(output);
        return output.ToArray();
    }

    private static void ReplaceCellPlaceholders(IXLCell cell, Dictionary<string, string> replacements)
    {
        var text = cell.GetString();
        foreach (var (placeholder, value) in replacements)
            text = text.Replace(placeholder, value);
        cell.Value = text;
    }

    private static MemoryStream LoadTemplate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(TemplateResourceName)
            ?? throw new InvalidOperationException(
                $"Template không tìm thấy: {TemplateResourceName}");

        var ms = new MemoryStream();
        stream.CopyTo(ms);
        ms.Position = 0;
        return ms;
    }
}
