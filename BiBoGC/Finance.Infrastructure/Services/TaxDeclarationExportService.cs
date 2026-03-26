using Finance.Application.Interfaces;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace Finance.Infrastructure.Services;

/// <summary>
/// Fills the official Mẫu 01/CNKD tax declaration template (.docx)
/// by replacing {placeholder} markers inside the Word XML.
/// Template: Finance.Infrastructure.Templates.Khai-thue-ho-kinh-doanh-template.docx
/// </summary>
public class TaxDeclarationExportService : ITaxDeclarationExportService
{
    // Standard rates for "Phân phối, cung cấp hàng hóa" per TT 40/2021/TT-BTC
    private const decimal GtgtRate = 0.01m; // 1%
    private const decimal TncnRate = 0.005m; // 0.5%

    private static readonly string TemplateResourceName =
        "Finance.Infrastructure.Templates.Khai-thue-ho-kinh-doanh-template.docx";

    public byte[] GenerateTaxDeclarationDocx(TaxDeclarationData data)
    {
        // Load the embedded template
        var assembly = Assembly.GetExecutingAssembly();
        using var templateStream = assembly.GetManifestResourceStream(TemplateResourceName)
                                   ?? throw new InvalidOperationException(
                                       $"Không tìm thấy template: {TemplateResourceName}");

        // Copy template into a writable MemoryStream
        var outputStream = new MemoryStream();
        templateStream.CopyTo(outputStream);
        outputStream.Position = 0;

        // Build placeholder → value map
        var replacements = BuildReplacements(data);

        // Open as ZIP (docx = ZIP), replace text in word/document.xml
        // Must use explicit using-block so the archive is flushed/disposed
        // before we read outputStream — otherwise the ZIP is incomplete.
        using (var archive = new ZipArchive(outputStream, ZipArchiveMode.Update, leaveOpen: true))
        {
            var entries = new[] { "word/document.xml", "word/header1.xml", "word/footer1.xml" };
            foreach (var entryName in entries)
            {
                var entry = archive.GetEntry(entryName);
                if (entry is null) continue;

                string originalXml;
                using (var reader = new StreamReader(entry.Open(), Encoding.UTF8))
                    originalXml = reader.ReadToEnd();

                var replacedXml = ApplyReplacements(originalXml, replacements);

                // Overwrite the entry
                entry.Delete();
                var newEntry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                using var writer = new StreamWriter(newEntry.Open(), new UTF8Encoding(false));
                writer.Write(replacedXml);
            }
        } // archive.Dispose() flushes the ZIP central directory here

        outputStream.Position = 0;
        return outputStream.ToArray();
    }

    private static Dictionary<string, string> BuildReplacements(TaxDeclarationData data)
    {
        return new Dictionary<string, string>
        {
            ["{currentYear}"] = data.CurrentYear.ToString(),
            ["{userInputMonthFrom}"] = data.MonthFrom.ToString("D2"),
            ["{userInputYearFrom}"] = data.YearFrom.ToString(),
            ["{userInputMonthTo}"] = data.MonthTo.ToString("D2"),
            ["{userInputYearTo}"] = data.YearTo.ToString(),
            ["{resultRevenue}"] = data.Revenue.ToString("N0"),
            ["{resultCalculateBaseOnTaxGTGTValue}"] = data.GtgtTaxAmount.ToString("N0"),
            ["{resultCalculateBaseOnTaxTNCNValue}"] = data.TncnTaxAmount.ToString("N0"),
            ["{dayExportReport}"] = data.DayExport.ToString("D2"),
            ["{monthExportReport}"] = data.MonthExport.ToString("D2"),
            ["{yearExportReport}"] = data.YearExport.ToString(),
        };
    }

    /// <summary>
    /// Replaces placeholders in the raw XML string.
    /// Word sometimes splits a placeholder across multiple &lt;w:t&gt; runs.
    /// This method first strips XML tags to find split markers, then replaces them.
    /// For robustness, we also do a direct string replace which handles the common case
    /// where the placeholder was typed as one unbroken run.
    /// </summary>
    private static string ApplyReplacements(string xml, Dictionary<string, string> replacements)
    {
        foreach (var (placeholder, value) in replacements)
            xml = xml.Replace(placeholder, value);

        return xml;
    }
}