using Shared.Domain.Enums;

namespace Shared.Application.Interfaces;

/// <summary>
/// Fire-and-forget notification service available to all Application layers.
/// Implementations must not throw — failures are logged and swallowed so the
/// calling operation is never broken by a notification error.
/// </summary>
public interface INotificationService
{
    Task NotifyAsync(
        string title,
        string message,
        NotificationType type,
        NotificationRole role,
        Guid? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default);
}