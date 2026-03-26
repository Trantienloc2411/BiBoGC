namespace Notification.Application.DTOs;

public class NotificationDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string Message { get; init; } = null!;
    public string Type { get; init; } = null!;
    public string Role { get; init; } = null!;
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid? ReferenceId { get; init; }
    public string? ReferenceType { get; init; }
}
