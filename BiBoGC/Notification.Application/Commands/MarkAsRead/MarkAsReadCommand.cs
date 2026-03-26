using MediatR;
using Shared.Application.Common;

namespace Notification.Application.Commands.MarkAsRead;

public record MarkAsReadCommand(Guid NotificationId) : IRequest<Result<bool>>;
