using MediatR;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Shared.Application.Common;

namespace Notification.Application.Queries.GetNotifications;

public class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, Result<NotificationListDto>>
{
    private readonly INotificationRepository _repository;

    public GetNotificationsQueryHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<NotificationListDto>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var totalCount = await _repository.CountByRoleAsync(request.Role, request.UnreadOnly, cancellationToken);
        var items = await _repository.GetByRoleAsync(
            request.Role,
            request.UnreadOnly,
            page,
            pageSize,
            cancellationToken);

        var unreadCount = await _repository.CountUnreadAsync(request.Role, cancellationToken);

        var dtos = items.Select(n => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type.ToString(),
            Role = n.Role.ToString(),
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            ReferenceId = n.ReferenceId,
            ReferenceType = n.ReferenceType
        }).ToList();

        return Result<NotificationListDto>.Success(new NotificationListDto
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            UnreadCount = unreadCount
        });
    }
}
