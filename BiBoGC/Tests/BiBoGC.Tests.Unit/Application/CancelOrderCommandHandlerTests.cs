using FluentAssertions;
using Moq;
using Sale.Application.Commands.CancelOrder;
using Sale.Application.Interfaces;
using Sale.Domain.Domain;
using Sale.Domain.Enum;

namespace BiBoGC.Tests.Unit.Application;

public class CancelOrderCommandHandlerTests
{
    private readonly Mock<ISalesOrderRepository> _orderRepoMock = new();

    private CancelOrderCommandHandler CreateHandler()
    {
        return new CancelOrderCommandHandler(_orderRepoMock.Object);
    }

    private static SalesOrder DraftOrder()
    {
        var order = new SalesOrder("ORD-001", PaymentMethod.Cash);
        return order;
    }

    // ── Order not found ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_OrderNotFound_ReturnsFailure()
    {
        _orderRepoMock
            .Setup(r => r.GetByIdWithItemsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((SalesOrder?)null);

        var result = await CreateHandler().Handle(
            new CancelOrderCommand(Guid.NewGuid(), "wrong order"), default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Không tìm thấy"));
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DraftOrder_CancelsSuccessfully()
    {
        var order = DraftOrder();
        _orderRepoMock
            .Setup(r => r.GetByIdWithItemsAsync(order.Id, default))
            .ReturnsAsync(order);

        var result = await CreateHandler().Handle(
            new CancelOrderCommand(order.Id, "Khách hủy"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task Handle_Cancellation_PersistsToRepository()
    {
        var order = DraftOrder();
        _orderRepoMock
            .Setup(r => r.GetByIdWithItemsAsync(order.Id, default))
            .ReturnsAsync(order);

        await CreateHandler().Handle(
            new CancelOrderCommand(order.Id, "Test"), default);

        _orderRepoMock.Verify(r => r.UpdateAsync(
            It.Is<SalesOrder>(o => o.Status == OrderStatus.Cancelled),
            default), Times.Once);
    }

    // ── Already completed order cannot be cancelled ───────────────────────────

    [Fact]
    public async Task Handle_CompletedOrder_ReturnsFailure()
    {
        var order = DraftOrder();
        order.AddItem(Guid.NewGuid(), Guid.NewGuid(), null,
            "Sản phẩm", "Loại 1", "SKU-X", "chai", 1, 10_000m);
        order.ApplyTax(0m);
        order.Complete(10_000m); // transition to Completed

        _orderRepoMock
            .Setup(r => r.GetByIdWithItemsAsync(order.Id, default))
            .ReturnsAsync(order);

        var result = await CreateHandler().Handle(
            new CancelOrderCommand(order.Id, "Test"), default);

        result.IsSuccess.Should().BeFalse();
    }

    // ── Already cancelled order ───────────────────────────────────────────────

    [Fact]
    public async Task Handle_AlreadyCancelledOrder_ReturnsFailure()
    {
        var order = DraftOrder();
        order.Cancel("First cancel");

        _orderRepoMock
            .Setup(r => r.GetByIdWithItemsAsync(order.Id, default))
            .ReturnsAsync(order);

        var result = await CreateHandler().Handle(
            new CancelOrderCommand(order.Id, "Second cancel"), default);

        result.IsSuccess.Should().BeFalse();
    }
}