using NotificationEntity = Notification.Domain.Entities.Notification;
using Shared.Domain.Enums;

namespace Notification.Application.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(NotificationEntity notification, CancellationToken ct = default);
    Task<NotificationEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<NotificationEntity>> GetByRoleAsync(
        NotificationRole? role,
        bool unreadOnly,
        int page,
        int pageSize,
        CancellationToken ct = default);
    Task<int> CountByRoleAsync(NotificationRole? role, bool unreadOnly, CancellationToken ct = default);
    Task<int> CountUnreadAsync(NotificationRole? role, CancellationToken ct = default);
    Task MarkAllAsReadAsync(NotificationRole role, CancellationToken ct = default);
}
