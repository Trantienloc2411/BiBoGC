using MediatR;
using Shared.Application.Common;
using Shared.Domain.Enums;

namespace Notification.Application.Commands.MarkAllAsRead;

public record MarkAllAsReadCommand(NotificationRole Role) : IRequest<Result<bool>>;
