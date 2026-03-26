namespace Notification.Application.Interfaces;

public interface INotificationUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
