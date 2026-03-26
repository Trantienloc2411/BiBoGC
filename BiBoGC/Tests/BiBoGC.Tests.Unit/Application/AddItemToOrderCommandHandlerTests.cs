using FluentAssertions;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.ValueObjects;
using Moq;
using Sale.Application.Commands.AddItemToOrder;
using Sale.Application.Interfaces;
using Sale.Domain.Domain;
using Sale.Domain.Enum;

namespace BiBoGC.Tests.Unit.Application;

public class AddItemToOrderCommandHandlerTests
{
    private readonly Mock<ISalesOrderRepository> _orderRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IProductVariantRepository> _variantRepoMock = new();

    private AddItemToOrderCommandHandler CreateHandler()
    {
        return new AddItemToOrderCommandHandler(_orderRepoMock.Object, _productRepoMock.Object,
            _variantRepoMock.Object);
    }

    private static SalesOrder DraftOrder()
    {
        return new SalesOrder("ORD-001", PaymentMethod.Cash);
    }

    private static Product BuildProduct(Guid? id = null)
    {
        return new Product(
            "Coca Cola", "Mô tả",
            ProductStatuses.Active,
            new Sku("SKU-COCA"),
            false,
            Units.Lon,
            new Money(10_000m),
            null);
    }

    private static ProductVariant BuildVariant(Guid productId)
    {
        return new ProductVariant(productId, "SKU-COCA-330ML", "Lon 330ml",
            Units.Lon, 1, new Money(12_000m), null, null);
    }

    // ── Order not found ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_OrderNotFound_ReturnsFailure()
    {
        _orderRepoMock
            .Setup(r => r.GetByIdWithItemsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((SalesOrder?)null);

        var result = await CreateHandler().Handle(
            new AddItemToOrderCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, 1), default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Không tìm thấy đơn hàng"));
    }

    // ── Product not found ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ProductNotFound_ReturnsFailure()
    {
        var order = DraftOrder();
        _orderRepoMock.Setup(r => r.GetByIdWithItemsAsync(order.Id, default)).ReturnsAsync(order);
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Product?)null);

        var result = await CreateHandler().Handle(
            new AddItemToOrderCommand(order.Id, Guid.NewGuid(), Guid.NewGuid(), null, 1), default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Không tìm thấy sản phẩm"));
    }

    // ── Variant not found ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_VariantNotFound_ReturnsFailure()
    {
        var order = DraftOrder();
        var product = BuildProduct();
        _orderRepoMock.Setup(r => r.GetByIdWithItemsAsync(order.Id, default)).ReturnsAsync(order);
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync(product);
        _variantRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync((ProductVariant?)null);

        var result = await CreateHandler().Handle(
            new AddItemToOrderCommand(order.Id, product.Id, Guid.NewGuid(), null, 1), default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("biến thể"));
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidRequest_AddsItemAndReturnsOrder()
    {
        var order = DraftOrder();
        var product = BuildProduct();
        var variant = BuildVariant(product.Id);

        _orderRepoMock.Setup(r => r.GetByIdWithItemsAsync(order.Id, default)).ReturnsAsync(order);
        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);
        _variantRepoMock.Setup(r => r.GetByIdAsync(product.Id, variant.Id, default)).ReturnsAsync(variant);

        var result = await CreateHandler().Handle(
            new AddItemToOrderCommand(order.Id, product.Id, variant.Id, null, 3), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().ContainSingle(i => i.Quantity == 3 && i.UnitPrice == 12_000m);
    }

    [Fact]
    public async Task Handle_ValidRequest_PersistsOrderUpdate()
    {
        var order = DraftOrder();
        var product = BuildProduct();
        var variant = BuildVariant(product.Id);

        _orderRepoMock.Setup(r => r.GetByIdWithItemsAsync(order.Id, default)).ReturnsAsync(order);
        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);
        _variantRepoMock.Setup(r => r.GetByIdAsync(product.Id, variant.Id, default)).ReturnsAsync(variant);

        await CreateHandler().Handle(
            new AddItemToOrderCommand(order.Id, product.Id, variant.Id, null, 2), default);

        _orderRepoMock.Verify(r => r.UpdateAsync(It.IsAny<SalesOrder>(), default), Times.Once);
    }
}