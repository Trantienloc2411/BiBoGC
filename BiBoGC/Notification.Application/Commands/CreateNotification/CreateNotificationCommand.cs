using MediatR;
using Shared.Application.Common;
using Shared.Domain.Enums;

namespace Notification.Application.Commands.CreateNotification;

public record CreateNotificationCommand(
    string Title,
    string Message,
    NotificationType Type,
    NotificationRole Role,
    Guid? ReferenceId = null,
    string? ReferenceType = null
) : IRequest<Result<Guid>>;
