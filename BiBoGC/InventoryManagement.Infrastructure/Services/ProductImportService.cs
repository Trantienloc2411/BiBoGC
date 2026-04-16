using System.Text.RegularExpressions;
using ClosedXML.Excel;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.ValueObjects;
using Shared.Application.Common;

namespace InventoryManagement.Infrastructure.Services;

/// <summary>
/// Generates an Excel import template and processes uploaded Excel files to create products.
/// </summary>
public partial class ProductImportService : IProductImportService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    // ── Unit name → Units enum value ────────────────────────────────────────
    private static readonly Dictionary<string, int> UnitsMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Cái"] = 1, ["Cai"] = 1, ["Pcs"] = 1,
        ["Hộp"] = 2, ["Hop"] = 2,
        ["Chai"] = 3,
        ["Lon"] = 4,
        ["Gói"] = 5, ["Goi"] = 5,
        ["Bịch"] = 6, ["Bich"] = 6,
        ["Lốc"] = 7, ["Loc"] = 7,
        ["Thùng"] = 8, ["Thung"] = 8,
        ["Cuộn"] = 9, ["Cuon"] = 9,
        ["Vỉ"] = 10, ["Vi"] = 10,
        ["Cây"] = 11, ["Cay"] = 11,
        ["Thanh"] = 12,
        ["Túi"] = 13, ["Tui"] = 13,
        ["Bộ"] = 14, ["Bo"] = 14,
        ["Đôi"] = 15, ["Doi"] = 15,
        ["Cân"] = 16, ["Can"] = 16,
        ["Kg"] = 21,
        ["Lạng"] = 22, ["Lang"] = 22,
        ["Lít"] = 31, ["Lit"] = 31,
        ["Quả"] = 40, ["Qua"] = 40,
        ["Trái"] = 41, ["Trai"] = 41
    };

    private static readonly string[] UnitOptions =
    [
        "Cái", "Hộp", "Chai", "Lon", "Gói", "Bịch", "Lốc", "Thùng",
        "Cuộn", "Vỉ", "Cây", "Thanh", "Túi", "Bộ", "Đôi", "Cân",
        "Kg", "Lạng", "Lít", "Quả", "Trái"
    ];

    [GeneratedRegex(@"^[A-Z0-9\-_]+$")]
    private static partial Regex SkuRegex();

    public ProductImportService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    // ── Template generation ──────────────────────────────────────────────────

    public byte[] GenerateImportTemplate()
    {
        using var workbook = new XLWorkbook();

        // ── Hidden option sheet ─────────────────────────────────────────────
        var optSheet = workbook.AddWorksheet("__Options__");
        optSheet.Visibility = XLWorksheetVisibility.Hidden;
        for (int i = 0; i < UnitOptions.Length; i++)
            optSheet.Cell(i + 1, 1).Value = UnitOptions[i];
        optSheet.Cell(1, 3).Value = "Có";
        optSheet.Cell(2, 3).Value = "Không";

        // ── Data sheet ──────────────────────────────────────────────────────
        var sheet = workbook.AddWorksheet("Danh sách sản phẩm");

        // Column headers
        var headers = new[]
        {
            "Tên sản phẩm (*)", "Mã SKU (*)", "Giá bán (*) (VND)",
            "Đơn vị tính (*)", "Danh mục", "Mô tả",
            "Ngưỡng cảnh báo tồn kho", "Theo dõi lô hàng"
        };
        for (int i = 0; i < headers.Length; i++)
        {
            var hCell = sheet.Cell(1, i + 1);
            hCell.Value = headers[i];
            hCell.Style.Font.Bold = true;
            hCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
            hCell.Style.Font.FontColor = XLColor.White;
            hCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Notes / instruction row
        var notes = new[]
        {
            "Tối đa 200 ký tự",
            "Tối đa 20 ký tự, chữ in hoa, số, - hoặc _",
            "Số >= 0",
            "Chọn từ danh sách",
            "Tên danh mục trong hệ thống (không bắt buộc)",
            "Tối đa 1 000 ký tự (không bắt buộc)",
            "Số nguyên dương (không bắt buộc)",
            "Có / Không (mặc định: Không)"
        };
        for (int i = 0; i < notes.Length; i++)
        {
            var nCell = sheet.Cell(2, i + 1);
            nCell.Value = notes[i];
            nCell.Style.Font.Italic = true;
            nCell.Style.Font.FontColor = XLColor.Gray;
            nCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F3F4F6");
        }

        // Sample data row (row 3)
        sheet.Cell(3, 1).Value = "Nước suối Aquafina 500ml";
        sheet.Cell(3, 2).Value = "AQUA-500ML";
        sheet.Cell(3, 3).Value = 8000;
        sheet.Cell(3, 4).Value = "Chai";
        sheet.Cell(3, 5).Value = "Nước giải khát";
        sheet.Cell(3, 6).Value = "Nước khoáng tinh khiết Aquafina";
        sheet.Cell(3, 7).Value = 50;
        sheet.Cell(3, 8).Value = "Không";
        for (int c = 1; c <= 8; c++)
            sheet.Cell(3, c).Style.Fill.BackgroundColor = XLColor.FromHtml("#DBEAFE");

        // Data validation: Unit dropdown (rows 3–1000, col 4)
        var unitValidation = sheet.Range(3, 4, 1000, 4).SetDataValidation();
        unitValidation.AllowedValues = XLAllowedValues.List;
        unitValidation.List(optSheet.Range(1, 1, UnitOptions.Length, 1));
        unitValidation.ShowErrorMessage = true;
        unitValidation.ErrorTitle = "Đơn vị không hợp lệ";
        unitValidation.ErrorMessage = "Vui lòng chọn đơn vị từ danh sách thả xuống.";
        unitValidation.ShowInputMessage = true;
        unitValidation.InputTitle = "Đơn vị tính";
        unitValidation.InputMessage = "Chọn một đơn vị tính từ danh sách.";

        // Data validation: Batch-tracking Yes/No dropdown (col 8)
        var btValidation = sheet.Range(3, 8, 1000, 8).SetDataValidation();
        btValidation.AllowedValues = XLAllowedValues.List;
        btValidation.List(optSheet.Range(1, 3, 2, 3));
        btValidation.ShowErrorMessage = true;
        btValidation.ErrorTitle = "Giá trị không hợp lệ";
        btValidation.ErrorMessage = "Nhập 'Có' hoặc 'Không'.";

        // Format: price column as number
        sheet.Column(3).Style.NumberFormat.Format = "#,##0";

        // Column widths
        int[] widths = [35, 22, 20, 18, 25, 45, 28, 20];
        for (int i = 0; i < widths.Length; i++)
            sheet.Column(i + 1).Width = widths[i];

        sheet.SheetView.FreezeRows(2);
        sheet.TabColor = XLColor.RoyalBlue;

        // ── Instructions sheet ───────────────────────────────────────────────
        var instr = workbook.AddWorksheet("Hướng dẫn");
        instr.TabColor = XLColor.Orange;

        instr.Cell(1, 1).Value = "HƯỚNG DẪN NHẬP SẢN PHẨM QUA EXCEL";
        instr.Cell(1, 1).Style.Font.Bold = true;
        instr.Cell(1, 1).Style.Font.FontSize = 14;
        instr.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml("#1D4ED8");

        var rows = new (string Col, string Desc)[]
        {
            ("", ""),
            ("CỘT", "QUY TẮC"),
            ("Tên sản phẩm (*)", "Bắt buộc. Tối đa 200 ký tự."),
            ("Mã SKU (*)", "Bắt buộc. Tối đa 20 ký tự. Chỉ dùng chữ IN HOA, số (0-9), dấu gạch ngang (-) và gạch dưới (_). Ví dụ: AQUA-500ML"),
            ("Giá bán (*)", "Bắt buộc. Số >= 0 (VND). Không nhập dấu phẩy ngăn cách hàng nghìn."),
            ("Đơn vị tính (*)", "Bắt buộc. Chọn từ danh sách thả xuống: " + string.Join(", ", UnitOptions) + "."),
            ("Danh mục", "Không bắt buộc. Nhập chính xác tên danh mục đã có trong hệ thống. Nếu không khớp, sản phẩm sẽ không có danh mục."),
            ("Mô tả", "Không bắt buộc. Tối đa 1 000 ký tự."),
            ("Ngưỡng cảnh báo tồn kho", "Không bắt buộc. Số nguyên không âm. Khi tồn kho < ngưỡng này sẽ nhận cảnh báo."),
            ("Theo dõi lô hàng", "Không bắt buộc. 'Có' = bật theo dõi lô (ngày SX, hạn dùng). Mặc định là 'Không'."),
            ("", ""),
            ("LƯU Ý QUAN TRỌNG", ""),
            ("1.", "Hàng 1 (tiêu đề) và hàng 2 (ghi chú) tự động bị bỏ qua. Dữ liệu bắt đầu từ hàng 3."),
            ("2.", "Hàng mẫu màu xanh (hàng 3) chỉ để minh họa — hãy xoá hoặc thay bằng dữ liệu thực."),
            ("3.", "Mã SKU không được trùng với sản phẩm đã có trong hệ thống hoặc với hàng khác trong cùng file."),
            ("4.", "Các hàng hợp lệ được nhập; các hàng có lỗi bị bỏ qua và báo cáo sau khi nhập."),
            ("5.", "File phải là định dạng .xlsx (Excel 2007 trở lên)."),
        };

        for (int i = 0; i < rows.Length; i++)
        {
            var (col, desc) = rows[i];
            instr.Cell(i + 2, 1).Value = col;
            instr.Cell(i + 2, 2).Value = desc;

            if (col is "CỘT" or "LƯU Ý QUAN TRỌNG")
            {
                for (int c = 1; c <= 2; c++)
                {
                    instr.Cell(i + 2, c).Style.Font.Bold = true;
                    instr.Cell(i + 2, c).Style.Fill.BackgroundColor = XLColor.FromHtml("#DBEAFE");
                }
            }
        }
        instr.Column(1).Width = 32;
        instr.Column(2).Width = 85;

        // Active the data sheet by default
        workbook.Worksheets.First(w => w.Name == "Danh sách sản phẩm").SetTabActive();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    // ── Import ───────────────────────────────────────────────────────────────

    public async Task<Result<ImportProductsResultDto>> ImportFromExcelAsync(
        Stream fileStream,
        bool dryRun = false,
        CancellationToken cancellationToken = default)
    {
        // ── 1. Parse Excel ───────────────────────────────────────────────────
        List<ParsedRow> parsed;
        try
        {
            parsed = ParseExcel(fileStream);
        }
        catch (Exception ex)
        {
            return Result<ImportProductsResultDto>.Failure(
                $"Không thể đọc file Excel: {ex.Message}");
        }

        if (parsed.Count == 0)
            return Result<ImportProductsResultDto>.Failure(
                "File không có dữ liệu. Vui lòng điền dữ liệu từ hàng 3 trở đi.");

        // ── 2. Pre-fetch all categories for name lookup ──────────────────────
        var allCategories = await _categoryRepository.GetAllAsync(false, cancellationToken);
        var catByName = allCategories
            .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        // ── 3. Validate every row, then import valid ones ────────────────────
        var seenSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var rowErrors = new List<ImportProductRowError>();
        var importedSkus = new List<string>();
        int successful = 0;

        foreach (var row in parsed)
        {
            var errors = await ValidateRowAsync(row, seenSkus, cancellationToken);

            if (errors.Count > 0)
            {
                rowErrors.Add(new ImportProductRowError
                {
                    RowNumber = row.RowNumber,
                    Name = row.Name,
                    Sku = row.Sku,
                    Errors = errors
                });
                continue;
            }

            // Track SKU so duplicates within the same file are caught
            seenSkus.Add(row.Sku!.Trim().ToUpper());
            successful++;

            if (!dryRun)
            {
                try
                {
                    var sku = row.Sku!.Trim().ToUpper();
                    var unitValue = (Units)UnitsMap[row.Unit!.Trim()];
                    Guid? categoryId = null;
                    if (!string.IsNullOrWhiteSpace(row.CategoryName) &&
                        catByName.TryGetValue(row.CategoryName.Trim(), out var cat))
                        categoryId = cat.Id;

                    var product = new Product(
                        row.Name!.Trim(),
                        skuGeneral: new Sku(sku),
                        basePrice: new Money(row.Price!.Value),
                        baseUnits: unitValue,
                        description: row.Description?.Trim() ?? string.Empty,
                        status: ProductStatuses.Active,
                        requiresBatchTracking: row.RequiresBatchTracking,
                        lowStockThreshold: row.LowStockThreshold,
                        categoryId: categoryId
                    );

                    // Add default variant (mirrors CreateProductCommandHandler)
                    product.AddProductVariant(
                        $"{sku}-DEFAULT",
                        product.Id,
                        $"{row.Name!.Trim()} 1 {unitValue}",
                        unitValue,
                        1,
                        new Money(row.Price!.Value),
                        1,
                        null,
                        null
                    );

                    await _productRepository.AddAsync(product, cancellationToken);
                    importedSkus.Add(sku);
                }
                catch (Exception ex)
                {
                    // Downgrade from "successful" count since the DB write failed
                    successful--;
                    rowErrors.Add(new ImportProductRowError
                    {
                        RowNumber = row.RowNumber,
                        Name = row.Name,
                        Sku = row.Sku,
                        Errors = [$"Lỗi khi tạo sản phẩm: {ex.Message}"]
                    });
                }
            }
        }

        return Result<ImportProductsResultDto>.Success(new ImportProductsResultDto
        {
            TotalRows = parsed.Count,
            Successful = successful,
            Failed = rowErrors.Count,
            IsDryRun = dryRun,
            RowErrors = rowErrors,
            ImportedSkus = dryRun ? [] : importedSkus
        });
    }

    // ── Excel parsing ────────────────────────────────────────────────────────

    private static List<ParsedRow> ParseExcel(Stream stream)
    {
        var result = new List<ParsedRow>();
        using var wb = new XLWorkbook(stream);
        // Use first non-hidden, non-option sheet
        var sheet = wb.Worksheets.First(w => w.Visibility == XLWorksheetVisibility.Visible);

        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 2;
        // Row 1 = headers, Row 2 = notes — data starts at row 3
        for (int r = 3; r <= lastRow; r++)
        {
            var row = sheet.Row(r);

            // Skip blank rows
            bool hasData = Enumerable.Range(1, 8)
                .Any(c => !string.IsNullOrWhiteSpace(row.Cell(c).GetString()));
            if (!hasData) continue;

            var priceRaw = row.Cell(3);
            decimal? price = priceRaw.DataType == XLDataType.Number
                ? (decimal)priceRaw.GetDouble()
                : decimal.TryParse(priceRaw.GetString().Trim(), out var p) ? p : null;

            var threshRaw = row.Cell(7);
            int? threshold = threshRaw.DataType == XLDataType.Number
                ? (int)threshRaw.GetDouble()
                : int.TryParse(threshRaw.GetString().Trim(), out var t) ? t : null;

            var btStr = row.Cell(8).GetString().Trim();
            bool batchTracking = btStr.Equals("Có", StringComparison.OrdinalIgnoreCase)
                || btStr.Equals("Co", StringComparison.OrdinalIgnoreCase)
                || btStr.Equals("Yes", StringComparison.OrdinalIgnoreCase)
                || btStr.Equals("true", StringComparison.OrdinalIgnoreCase)
                || btStr == "1";

            result.Add(new ParsedRow
            {
                RowNumber = r,
                Name = row.Cell(1).GetString(),
                Sku = row.Cell(2).GetString(),
                Price = price,
                Unit = row.Cell(4).GetString(),
                CategoryName = row.Cell(5).GetString(),
                Description = row.Cell(6).GetString(),
                LowStockThreshold = threshold,
                RequiresBatchTracking = batchTracking
            });
        }
        return result;
    }

    // ── Row validation ───────────────────────────────────────────────────────

    private async Task<List<string>> ValidateRowAsync(
        ParsedRow row,
        HashSet<string> seenSkusInBatch,
        CancellationToken ct)
    {
        var errors = new List<string>();

        // Name
        if (string.IsNullOrWhiteSpace(row.Name))
            errors.Add("Tên sản phẩm không được để trống.");
        else if (row.Name.Trim().Length > 200)
            errors.Add("Tên sản phẩm không được vượt quá 200 ký tự.");

        // SKU
        if (string.IsNullOrWhiteSpace(row.Sku))
        {
            errors.Add("Mã SKU không được để trống.");
        }
        else
        {
            var sku = row.Sku.Trim().ToUpper();
            if (sku.Length > 20)
                errors.Add($"Mã SKU '{sku}' không được vượt quá 20 ký tự.");
            else if (!SkuRegex().IsMatch(sku))
                errors.Add($"Mã SKU '{sku}' chỉ được chứa chữ IN HOA, số, dấu '-' hoặc '_'.");
            else if (seenSkusInBatch.Contains(sku))
                errors.Add($"Mã SKU '{sku}' bị trùng trong file (đã xuất hiện ở hàng trước).");
            else if (await _productRepository.SkuExistsAsync(sku, cancellationToken: ct))
                errors.Add($"Mã SKU '{sku}' đã tồn tại trong hệ thống.");
        }

        // Price
        if (row.Price is null)
            errors.Add("Giá bán không hợp lệ. Vui lòng nhập số >= 0.");
        else if (row.Price.Value < 0)
            errors.Add("Giá bán không được âm.");

        // Unit
        if (string.IsNullOrWhiteSpace(row.Unit))
            errors.Add("Đơn vị tính không được để trống.");
        else if (!UnitsMap.ContainsKey(row.Unit.Trim()))
            errors.Add($"Đơn vị tính '{row.Unit.Trim()}' không hợp lệ. Chọn từ: {string.Join(", ", UnitOptions)}.");

        // Description
        if (!string.IsNullOrEmpty(row.Description) && row.Description.Length > 1000)
            errors.Add("Mô tả không được vượt quá 1 000 ký tự.");

        // Low stock threshold
        if (row.LowStockThreshold.HasValue && row.LowStockThreshold.Value < 0)
            errors.Add("Ngưỡng cảnh báo tồn kho phải là số nguyên không âm.");

        return errors;
    }

    // ── Inner types ──────────────────────────────────────────────────────────

    private sealed class ParsedRow
    {
        public int RowNumber { get; set; }
        public string? Name { get; set; }
        public string? Sku { get; set; }
        public decimal? Price { get; set; }
        public string? Unit { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public int? LowStockThreshold { get; set; }
        public bool RequiresBatchTracking { get; set; }
    }
}
