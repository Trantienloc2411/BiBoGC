using Microsoft.EntityFrameworkCore;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Data;
using Shared.Domain.Enums;

namespace Notification.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Domain.Entities.Notification notification, CancellationToken ct = default)
    {
        await _context.Notifications.AddAsync(notification, ct);
    }

    public async Task<Domain.Entities.Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, ct);
    }

    public async Task<List<Domain.Entities.Notification>> GetByRoleAsync(
        NotificationRole? role,
        bool unreadOnly,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.Notifications.AsQueryable();

        // Role filter: exact match OR "Both" notifications are visible to everyone
        if (role.HasValue)
            query = query.Where(n =>
                n.Role == role.Value || n.Role == NotificationRole.Both);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> CountByRoleAsync(NotificationRole? role, bool unreadOnly, CancellationToken ct = default)
    {
        var query = _context.Notifications.AsQueryable();

        if (role.HasValue)
            query = query.Where(n => n.Role == role.Value || n.Role == NotificationRole.Both);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        return await query.CountAsync(ct);
    }

    public async Task<int> CountUnreadAsync(NotificationRole? role, CancellationToken ct = default)
    {
        var query = _context.Notifications.Where(n => !n.IsRead);

        if (role.HasValue)
            query = query.Where(n =>
                n.Role == role.Value || n.Role == NotificationRole.Both);

        return await query.CountAsync(ct);
    }

    public async Task MarkAllAsReadAsync(NotificationRole role, CancellationToken ct = default)
    {
        await _context.Notifications
            .Where(n => !n.IsRead && (n.Role == role || n.Role == NotificationRole.Both))
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.UpdatedAt, DateTime.UtcNow),
            ct);
    }
}
