using Notification.Application.Interfaces;
using Notification.Infrastructure.Data;

namespace Notification.Infrastructure.Persistence;

public class NotificationUnitOfWork : INotificationUnitOfWork
{
    private readonly NotificationDbContext _context;

    public NotificationUnitOfWork(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
