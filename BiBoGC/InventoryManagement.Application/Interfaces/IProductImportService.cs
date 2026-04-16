using InventoryManagement.Application.DTOs;
using Shared.Application.Common;

namespace InventoryManagement.Application.Interfaces;

public interface IProductImportService
{
    /// <summary>
    /// Generates and returns the bytes of a blank Excel import template that the
    /// user fills in and uploads to <see cref="ImportFromExcelAsync"/>.
    /// </summary>
    byte[] GenerateImportTemplate();

    /// <summary>
    /// Parses, validates and (optionally) persists products from an uploaded Excel file.
    /// </summary>
    /// <param name="fileStream">Readable stream of the .xlsx file.</param>
    /// <param name="dryRun">
    /// When <c>true</c> the rows are validated but nothing is written to the database.
    /// Use this to show a preview / error list to the user before committing.
    /// </param>
    /// <param name="cancellationToken"></param>
    Task<Result<ImportProductsResultDto>> ImportFromExcelAsync(
        Stream fileStream,
        bool dryRun = false,
        CancellationToken cancellationToken = default);
}
