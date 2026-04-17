namespace InventoryManagement.Application.DTOs;

/// <summary>Per-row validation or creation error returned from an import attempt.</summary>
public record ImportProductRowError
{
    public int RowNumber { get; init; }
    public string? Name { get; init; }
    public string? Sku { get; init; }
    public List<string> Errors { get; init; } = [];
}

/// <summary>Aggregate result returned by the import / dry-run endpoint.</summary>
public record ImportProductsResultDto
{
    /// <summary>Total data rows parsed from the file (excluding header and notes rows).</summary>
    public int TotalRows { get; init; }

    /// <summary>
    /// Number of rows that passed validation.
    /// On a dry-run this is the projected import count; on a real import it is the actual count.
    /// </summary>
    public int Successful { get; init; }

    /// <summary>Number of rows that failed validation or could not be created.</summary>
    public int Failed { get; init; }

    /// <summary>True when the call was a dry-run (no data was persisted).</summary>
    public bool IsDryRun { get; init; }

    /// <summary>Per-row error details for rows that failed.</summary>
    public IReadOnlyList<ImportProductRowError> RowErrors { get; init; } = [];

    /// <summary>SKUs that were successfully imported (only populated on a real import).</summary>
    public IReadOnlyList<string> ImportedSkus { get; init; } = [];
}
