using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.ImportProducts;

/// <summary>
/// Parses an Excel file and creates products for all valid rows.
/// Set <see cref="DryRun"/> = <c>true</c> to validate without persisting.
/// </summary>
public record ImportProductsCommand : IRequest<Result<ImportProductsResultDto>>
{
    public required Stream FileStream { get; init; }

    /// <summary>
    /// When true, rows are validated but nothing is written to the database.
    /// Returns the same <see cref="ImportProductsResultDto"/> so the caller can
    /// preview what will be imported (and which rows have errors) before committing.
    /// </summary>
    public bool DryRun { get; init; } = false;
}
