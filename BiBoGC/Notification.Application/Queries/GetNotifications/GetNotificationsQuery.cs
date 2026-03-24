using MediatR;
using Notification.Application.DTOs;
using Shared.Application.Common;
using Shared.Domain.Enums;

namespace Notification.Application.Queries.GetNotifications;

public record GetNotificationsQuery(
    NotificationRole? Role = null,
    bool UnreadOnly = false,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<NotificationListDto>>;
