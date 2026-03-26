using FluentAssertions;
using Shared.Domain.Enums;

namespace BiBoGC.Tests.Unit.Domain;

public class NotificationTests
{
    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_SetsAllProperties()
    {
        var n = Notification.Domain.Entities.Notification.Create(
            "Đơn hàng hoàn thành", "ORD-001 đã thanh toán.",
            NotificationType.Info, NotificationRole.Seller,
            Guid.NewGuid(), "SalesOrder");

        n.Title.Should().Be("Đơn hàng hoàn thành");
        n.Message.Should().Be("ORD-001 đã thanh toán.");
        n.Type.Should().Be(NotificationType.Info);
        n.Role.Should().Be(NotificationRole.Seller);
        n.IsRead.Should().BeFalse();
        n.Id.Should().NotBe(Guid.Empty);
        n.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(3));
        n.ReferenceType.Should().Be("SalesOrder");
    }

    [Fact]
    public void Create_TrimsTitle()
    {
        var n = Notification.Domain.Entities.Notification.Create(
            "  Tiêu đề  ", "Nội dung",
            NotificationType.Warning, NotificationRole.Admin);

        n.Title.Should().Be("Tiêu đề");
    }

    // ── Validation ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankTitle_Throws(string title)
    {
        var act = () => Notification.Domain.Entities.Notification.Create(
            title, "msg", NotificationType.Info, NotificationRole.Admin);

        act.Should().Throw<ArgumentException>().WithParameterName("title");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankMessage_Throws(string message)
    {
        var act = () => Notification.Domain.Entities.Notification.Create(
            "Tiêu đề", message, NotificationType.Info, NotificationRole.Admin);

        act.Should().Throw<ArgumentException>().WithParameterName("message");
    }

    [Fact]
    public void Create_TitleExceeds200Chars_Throws()
    {
        var longTitle = new string('A', 201);
        var act = () => Notification.Domain.Entities.Notification.Create(
            longTitle, "msg", NotificationType.Info, NotificationRole.Admin);

        act.Should().Throw<ArgumentException>().WithParameterName("title");
    }

    // ── MarkAsRead ────────────────────────────────────────────────────────────

    [Fact]
    public void MarkAsRead_UnreadNotification_SetsIsReadTrue()
    {
        var n = Notification.Domain.Entities.Notification.Create(
            "Test", "Test msg", NotificationType.Info, NotificationRole.Admin);

        n.MarkAsRead();

        n.IsRead.Should().BeTrue();
        n.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void MarkAsRead_AlreadyRead_IsIdempotent()
    {
        var n = Notification.Domain.Entities.Notification.Create(
            "Test", "msg", NotificationType.Info, NotificationRole.Admin);
        n.MarkAsRead();
        var firstUpdatedAt = n.UpdatedAt;

        // Call again — should not throw or change UpdatedAt
        n.MarkAsRead();

        n.IsRead.Should().BeTrue();
        n.UpdatedAt.Should().Be(firstUpdatedAt);
    }

    // ── Type coverage ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData(NotificationType.Info)]
    [InlineData(NotificationType.Warning)]
    [InlineData(NotificationType.Error)]
    public void Create_AllNotificationTypes_Work(NotificationType type)
    {
        var n = Notification.Domain.Entities.Notification.Create(
            "T", "M", type, NotificationRole.Both);
        n.Type.Should().Be(type);
    }

    [Theory]
    [InlineData(NotificationRole.Admin)]
    [InlineData(NotificationRole.Seller)]
    [InlineData(NotificationRole.Both)]
    public void Create_AllRoles_Work(NotificationRole role)
    {
        var n = Notification.Domain.Entities.Notification.Create(
            "T", "M", NotificationType.Info, role);
        n.Role.Should().Be(role);
    }

    // ── Optional fields ───────────────────────────────────────────────────────

    [Fact]
    public void Create_WithoutReference_ReferenceFieldsAreNull()
    {
        var n = Notification.Domain.Entities.Notification.Create(
            "T", "M", NotificationType.Info, NotificationRole.Admin);

        n.ReferenceId.Should().BeNull();
        n.ReferenceType.Should().BeNull();
    }
}