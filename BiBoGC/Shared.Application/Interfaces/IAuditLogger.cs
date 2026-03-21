namespace Shared.Application.Interfaces;

/// <summary>
/// Thin audit logging interface available to all Application layers.
/// Implementations persist logs without breaking the calling operation.
/// </summary>
public interface IAuditLogger
{
    Task LogAsync(
        string action,
        bool isSuccess,
        Guid? userId = null,
        string? username = null,
        string? description = null,
        CancellationToken cancellationToken = default);
}
