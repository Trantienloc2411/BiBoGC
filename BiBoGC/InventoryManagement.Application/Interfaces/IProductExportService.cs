using Shared.Application.Common;

namespace InventoryManagement.Application.Interfaces;

public interface IProductExportService
{
    /// <summary>
    /// Fetches all active products from the database and exports them into the
    /// ProductExists.xlsx template (Biên bản kiểm kê hàng tồn kho).
    /// Template capacity: 6 data rows per file. Returns ZIP when exceeded.
    /// </summary>
    Task<ExportFileResult> ExportExistingProductsAsync(CancellationToken cancellationToken = default);
}
