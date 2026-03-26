using Shared.Domain.Common;
using Shared.Domain.Enums;

namespace Notification.Domain.Entities;

public class Notification : BaseEntity
{
    private Notification() { }

    public string Title { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public NotificationType Type { get; private set; }
    public NotificationRole Role { get; private set; }
    public bool IsRead { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public string? ReferenceType { get; private set; }

    public static Notification Create(
        string title,
        string message,
        NotificationType type,
        NotificationRole role,
        Guid? referenceId = null,
        string? referenceType = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Tiêu đề không được để trống.", nameof(title));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Nội dung thông báo không được để trống.", nameof(message));
        if (title.Length > 200)
            throw new ArgumentException("Tiêu đề không được vượt quá 200 ký tự.", nameof(title));

        return new Notification
        {
            Title = title.Trim(),
            Message = message.Trim(),
            Type = type,
            Role = role,
            IsRead = false,
            ReferenceId = referenceId,
            ReferenceType = referenceType?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsRead()
    {
        if (IsRead) return;
        IsRead = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
