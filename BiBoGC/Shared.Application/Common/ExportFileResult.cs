namespace Shared.Application.Common;

/// <summary>
/// Represents the result of a file export operation.
/// Data contains either a single Excel file or a ZIP archive.
/// </summary>
public record ExportFileResult(byte[] Data, bool IsZip);
