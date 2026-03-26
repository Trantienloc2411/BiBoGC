using FluentAssertions;
using Moq;
using Notification.Application.Commands.CreateNotification;
using Notification.Application.Interfaces;
using Shared.Domain.Enums;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace BiBoGC.Tests.Unit.Application;

public class CreateNotificationCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _repoMock = new();
    private readonly Mock<INotificationUnitOfWork> _uowMock = new();

    private CreateNotificationCommandHandler CreateHandler()
    {
        return new CreateNotificationCommandHandler(_repoMock.Object, _uowMock.Object);
    }

    private static CreateNotificationCommand ValidCommand(
        NotificationType type = NotificationType.Info,
        NotificationRole role = NotificationRole.Admin)
    {
        return new CreateNotificationCommand(
            "Test notification",
            "This is a test message.",
            type,
            role,
            null,
            null);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithId()
    {
        var result = await CreateHandler().Handle(ValidCommand(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryAdd()
    {
        await CreateHandler().Handle(ValidCommand(), default);

        _repoMock.Verify(r => r.AddAsync(
            It.Is<NotificationEntity>(n =>
                n.Title == "Test notification" &&
                n.Type == NotificationType.Info &&
                n.Role == NotificationRole.Admin &&
                !n.IsRead),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        await CreateHandler().Handle(ValidCommand(), default);

        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    // ── Type/role variants ────────────────────────────────────────────────────

    [Theory]
    [InlineData(NotificationType.Warning, NotificationRole.Seller)]
    [InlineData(NotificationType.Error, NotificationRole.Both)]
    [InlineData(NotificationType.Info, NotificationRole.Admin)]
    public async Task Handle_DifferentTypeAndRole_StoresCorrectly(
        NotificationType type, NotificationRole role)
    {
        await CreateHandler().Handle(ValidCommand(type, role), default);

        _repoMock.Verify(r => r.AddAsync(
            It.Is<NotificationEntity>(n => n.Type == type && n.Role == role),
            default), Times.Once);
    }

    // ── Reference fields ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithReference_StoresReferenceFields()
    {
        var refId = Guid.NewGuid();
        var cmd = new CreateNotificationCommand(
            "Order done", "msg", NotificationType.Info, NotificationRole.Seller,
            refId, "SalesOrder");

        await CreateHandler().Handle(cmd, default);

        _repoMock.Verify(r => r.AddAsync(
            It.Is<NotificationEntity>(n =>
                n.ReferenceId == refId && n.ReferenceType == "SalesOrder"),
            default), Times.Once);
    }

    // ── Domain validation propagates ──────────────────────────────────────────

    [Fact]
    public async Task Handle_BlankTitle_ThrowsFromDomain()
    {
        var cmd = new CreateNotificationCommand(
            "  ", "msg", NotificationType.Info, NotificationRole.Admin);

        var act = async () => await CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("title");
    }
}