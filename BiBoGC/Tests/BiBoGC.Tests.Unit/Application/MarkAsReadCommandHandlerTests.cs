using FluentAssertions;
using Moq;
using Notification.Application.Commands.MarkAsRead;
using Notification.Application.Interfaces;
using Shared.Domain.Enums;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace BiBoGC.Tests.Unit.Application;

public class MarkAsReadCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _repoMock = new();
    private readonly Mock<INotificationUnitOfWork> _uowMock = new();

    private MarkAsReadCommandHandler CreateHandler()
    {
        return new MarkAsReadCommandHandler(_repoMock.Object, _uowMock.Object);
    }

    private static NotificationEntity BuildNotification()
    {
        return NotificationEntity.Create(
            "Test", "Test message",
            NotificationType.Info, NotificationRole.Admin);
    }

    // ── Not found ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NotificationNotFound_ReturnsFailure()
    {
        _repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((NotificationEntity?)null);

        var result = await CreateHandler().Handle(
            new MarkAsReadCommand(Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Không tìm thấy thông báo"));
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ExistingNotification_MarksAsRead()
    {
        var notification = BuildNotification();
        _repoMock
            .Setup(r => r.GetByIdAsync(notification.Id, default))
            .ReturnsAsync(notification);

        var result = await CreateHandler().Handle(
            new MarkAsReadCommand(notification.Id), default);

        result.IsSuccess.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ExistingNotification_SavesChanges()
    {
        var notification = BuildNotification();
        _repoMock
            .Setup(r => r.GetByIdAsync(notification.Id, default))
            .ReturnsAsync(notification);

        await CreateHandler().Handle(new MarkAsReadCommand(notification.Id), default);

        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    // ── Already-read notification is idempotent ───────────────────────────────

    [Fact]
    public async Task Handle_AlreadyReadNotification_Succeeds()
    {
        var notification = BuildNotification();
        notification.MarkAsRead(); // pre-read

        _repoMock
            .Setup(r => r.GetByIdAsync(notification.Id, default))
            .ReturnsAsync(notification);

        var result = await CreateHandler().Handle(
            new MarkAsReadCommand(notification.Id), default);

        result.IsSuccess.Should().BeTrue();
    }
}