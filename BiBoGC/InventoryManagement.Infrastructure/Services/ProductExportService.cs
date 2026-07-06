using ClosedXML.Excel;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Common;
using System.IO.Compression;
using System.Reflection;

namespace InventoryManagement.Infrastructure.Services;

/// <summary>
/// Fills the ProductExists.xlsx template (Biên bản kiểm kê hàng tồn kho)
/// with all current products from the database.
/// Template capacity: 6 data rows (rows 11–16). Splits into multiple files when exceeded.
/// </summary>
public class ProductExportService : IProductExportService
{
    private const int DataStartRow = 11;
    private const int DataEndRow = 16;
    private const int MaxRowsPerFile = DataEndRow - DataStartRow + 1; // 6

    private static readonly string TemplateResourceName =
        "InventoryManagement.Infrastructure.Templates.ProductExists.xlsx";

    private readonly InventoryDbContext _context;

    public ProductExportService(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<ExportFileResult> ExportExistingProductsAsync(CancellationToken cancellationToken = default)
    {
        var rawProducts = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Batches)
            .Include(p => p.Variants)
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        var products = rawProducts.Select(p => new ProductExportRow(
                p.Name,
                p.SkuGeneral.Value,
                p.Category != null ? p.Category.Name : string.Empty,
                p.GetAvailableStock(),
                p.BaseUnits,
                p.Variants.OrderBy(v => v.DisplayOrder).FirstOrDefault()?.GetPricePerUnit() ?? 0))
            .ToList();

        var chunks = products
            .Select((item, i) => (item, i))
            .GroupBy(x => x.i / MaxRowsPerFile)
            .Select(g => g.Select(x => x.item).ToList())
            .ToList();

        if (chunks.Count == 0)
        {
            // No products — return blank template
            var blank = GenerateSingleFile([], fileIndex: 1);
            return new ExportFileResult(blank, IsZip: false);
        }

        if (chunks.Count == 1)
        {
            var data = GenerateSingleFile(chunks[0], fileIndex: 1);
            return new ExportFileResult(data, IsZip: false);
        }

        // Multiple files → ZIP
        using var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                var fileBytes = GenerateSingleFile(chunks[i], fileIndex: i + 1);
                var entryName = $"ProductExists_part{i + 1}.xlsx";
                var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                using var entryStream = entry.Open();
                entryStream.Write(fileBytes, 0, fileBytes.Length);
            }
        }

        return new ExportFileResult(zipStream.ToArray(), IsZip: true);
    }

    private byte[] GenerateSingleFile(List<ProductExportRow> chunk, int fileIndex)
    {
        using var templateStream = LoadTemplate();
        using var workbook = new XLWorkbook(templateStream);
        var ws = workbook.Worksheet(1);

        decimal grandTotal = 0;

        for (int i = 0; i < chunk.Count; i++)
        {
            var product = chunk[i];
            int row = DataStartRow + i;
            decimal lineTotal = product.AvailableStock * product.SalePrice;

            ws.Cell(row, 1).Value = i + 1;                          // STT
            ws.Cell(row, 2).Value = product.Sku;                    // Mã hàng hóa
            ws.Cell(row, 3).Value = product.Name;                   // Tên hàng hóa/Vật tư
            ws.Cell(row, 4).Value = GetUnitDisplayName(product.Unit); // Đơn vị tính
            ws.Cell(row, 5).Value = product.AvailableStock;         // Số lượng tồn kho
            ws.Cell(row, 6).Value = (double)product.SalePrice;      // Đơn giá (VND)
            ws.Cell(row, 7).Value = (double)lineTotal;              // Thành tiền (VND)
            ws.Cell(row, 8).Value = product.CategoryName;           // Ghi chú (Category)

            grandTotal += lineTotal;
        }

        // Row 17: Tổng — write total value in column G
        ws.Cell(17, 7).Value = (double)grandTotal;

        using var output = new MemoryStream();
        workbook.SaveAs(output);
        return output.ToArray();
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

    private static string GetUnitDisplayName(Units unit) => unit switch
    {
        Units.Pcs    => "Cái",
        Units.Hop    => "Hộp",
        Units.Chai   => "Chai",
        Units.Lon    => "Lon",
        Units.Goi    => "Gói",
        Units.Bich   => "Bịch",
        Units.Loc    => "Lốc",
        Units.Thung  => "Thùng",
        Units.Cuon   => "Cuộn",
        Units.Vi     => "Vỉ",
        Units.Cay    => "Cây",
        Units.Thanh  => "Thanh",
        Units.Tui    => "Túi",
        Units.Bo     => "Bộ",
        Units.Doi    => "Đôi",
        Units.Can    => "Cân",
        Units.Kg     => "Kg",
        Units.Lang   => "Lạng",
        Units.Lit    => "Lít",
        Units.Qua    => "Quả",
        Units.Trai   => "Trái",
        _            => unit.ToString()
    };

    private record ProductExportRow(
        string Name,
        string Sku,
        string CategoryName,
        int AvailableStock,
        Units Unit,
        decimal SalePrice);
}
