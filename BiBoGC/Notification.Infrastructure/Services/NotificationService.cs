using Microsoft.Extensions.Logging;
using Notification.Application.Interfaces;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Notification.Infrastructure.Services;

/// <summary>
/// Implements INotificationService from Shared.Application.
/// All errors are caught and logged — callers are never broken by a notification failure.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly INotificationUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository repository,
        INotificationUnitOfWork unitOfWork,
        ILogger<NotificationService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task NotifyAsync(
        string title,
        string message,
        NotificationType type,
        NotificationRole role,
        Guid? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = Domain.Entities.Notification.Create(
                title, message, type, role, referenceId, referenceType);

            await _repository.AddAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Fire-and-forget: notification failures must not break the calling operation
            _logger.LogError(ex,
                "Failed to create notification. Title={Title}, Type={Type}, Role={Role}",
                title, type, role);
        }
    }
}
