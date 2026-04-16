using Notification.Application.DTOs;

namespace Notification.Application.Interfaces;

/// <summary>
/// Abstraction for pushing notifications to connected clients in real time.
/// Implementations live in the API/host layer (e.g. SignalR).
/// </summary>
public interface IRealTimeNotificationPusher
{
    /// <summary>Pushes a notification to all clients subscribed to the given role group.</summary>
    Task PushAsync(NotificationDto notification, CancellationToken cancellationToken = default);
}
