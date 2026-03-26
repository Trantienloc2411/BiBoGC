namespace Notification.Application.DTOs;

public class NotificationListDto
{
    public List<NotificationDto> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public int UnreadCount { get; init; }
}
