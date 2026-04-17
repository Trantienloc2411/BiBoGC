using Microsoft.Extensions.Logging;
using Notification.Application.DTOs;
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
    private readonly IRealTimeNotificationPusher? _pusher;

    public NotificationService(
        INotificationRepository repository,
        INotificationUnitOfWork unitOfWork,
        ILogger<NotificationService> logger,
        IRealTimeNotificationPusher? pusher = null)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _pusher = pusher;
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

            // Push real-time to connected clients if pusher is available
            if (_pusher is not null)
            {
                var dto = new NotificationDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type.ToString(),
                    Role = notification.Role.ToString(),
                    IsRead = false,
                    CreatedAt = notification.CreatedAt,
                    ReferenceId = notification.ReferenceId,
                    ReferenceType = notification.ReferenceType
                };
                await _pusher.PushAsync(dto, cancellationToken);
            }
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
