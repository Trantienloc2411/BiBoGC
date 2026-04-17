using Microsoft.AspNetCore.SignalR;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Shared.Domain.Enums;

namespace BiBoGC.Hubs;

/// <summary>
/// Pushes notifications to SignalR groups that match the notification's role.
/// "Admin"  → group "Admin"
/// "Seller" → group "Seller"
/// "Both"   → groups "Admin" and "Seller"
/// </summary>
public class SignalRNotificationPusher : IRealTimeNotificationPusher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationPusher(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PushAsync(NotificationDto notification, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<NotificationRole>(notification.Role, out var role)) return;

        switch (role)
        {
            case NotificationRole.Admin:
                await _hubContext.Clients.Group("Admin")
                    .SendAsync("ReceiveNotification", notification, cancellationToken);
                break;

            case NotificationRole.Seller:
                await _hubContext.Clients.Group("Seller")
                    .SendAsync("ReceiveNotification", notification, cancellationToken);
                break;

            case NotificationRole.Both:
                await Task.WhenAll(
                    _hubContext.Clients.Group("Admin")
                        .SendAsync("ReceiveNotification", notification, cancellationToken),
                    _hubContext.Clients.Group("Seller")
                        .SendAsync("ReceiveNotification", notification, cancellationToken));
                break;
        }
    }
}
