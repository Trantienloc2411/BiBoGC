using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BiBoGC.Hubs;

/// <summary>
/// SignalR Hub for real-time notification push to Admin and Seller dashboards.
/// Clients join a role group (Admin | Seller) on connect.
/// Server pushes "ReceiveNotification" events to the matching group.
/// </summary>
[Authorize(Roles = "Administrator,Seller")]
public class NotificationHub : Hub
{
    /// <summary>
    /// Called by the client after connecting.
    /// Subscribes the connection to its role-based group so it only receives
    /// notifications targeted at that role (or Both).
    /// </summary>
    public async Task JoinRoleGroup(string role)
    {
        if (string.IsNullOrWhiteSpace(role)) return;
        await Groups.AddToGroupAsync(Context.ConnectionId, role);
    }
}
