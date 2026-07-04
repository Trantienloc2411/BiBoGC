using FluentAssertions;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.ValueObjects;
using Moq;
using Sale.Application.Commands.CompleteOrder;
using Sale.Application.Interfaces;
using Sale.Domain.Domain;
using Sale.Domain.Enum;
using Shared.Application.Interfaces;

namespace BiBoGC.Tests.Unit.Application;

public class CompleteOrderCommandHandlerTests
{
    private readonly Mock<IAuditLogger> _auditMock = new();
    private readonly Mock<INotificationService> _notificationMock = new();
    private readonly Mock<ISalesOrderRepository> _orderRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IStockTransactionRepository> _stockTxMock = new();
    private readonly Mock<ITaxConfigService> _taxMock = new();
    private readonly Mock<ISaleUnitOfWork> _uowMock = new();
    private readonly Mock<IProductVariantRepository> _variantRepoMock = new();

    private CompleteOrderCommandHandler CreateHandler()
    {
        return new CompleteOrderCommandHandler(
            _orderRepoMock.Object,
            _productRepoMock.Object,
            _variantRepoMock.Object,
            _stockTxMock.Object,
            _uowMock.Object,
            _auditMock.Object,
            _taxMock.Object,
            _notificationMock.Object);
    }

    private static SalesOrder DraftOrderWithItem(out Guid productId, out Guid variantId)
    {
        var order = new SalesOrder("ORD-001", PaymentMethod.Cash);
        productId = Guid.NewGuid();
        variantId = Guid.NewGuid();
        // 2 × 15,000 = SubTotal 30,000 (tax-inclusive)
        order.AddItem(productId, variantId, null,
            "Nước ngọt", "Chai 500ml", "SKU-001", "chai", 2, 15_000m);
        return order;
    }

    private Product BuildProduct(int availableStock = 100)
    {
        var product = new Product(
            "Nước ngọt", "Mô tả",
            ProductStatuses.Active,
            new Sku("SKU-001"),
            false,
            Units.Chai,
            null);

        if (availableStock > 0)
        {
            product.AddNewBatch("LOT-001", availableStock,
                DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(365));
            product.IncreaseStock(availableStock);
        }

        return product;
    }

    private static ProductVariant BuildVariant(Guid productId)
    {
        return new ProductVariant(productId,
            "SKU-001-1-chai",
            "Chai 500ml",
            Units.Chai,
            1,
            new Money(15_000m),
            null,
            null);
    }

    private void SetupHappyPath(Guid productId, Guid variantId, SalesOrder order, decimal vatRate = 0m)
    {
        _orderRepoMock.Setup(r => r.GetByIdWithItemsAsync(It.IsAny<Guid>(), default)).ReturnsAsync(order);
        _productRepoMock.Setup(r => r.GetByIdAsync(productId, default)).ReturnsAsync(BuildProduct(100));
        _variantRepoMock.Setup(r => r.GetByIdAsync(productId, variantId, default))
            .ReturnsAsync(BuildVariant(productId));
        _uowMock.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), default))
            .Returns((Func<Task> action, CancellationToken _) => action());
        _taxMock.Setup(t => t.GetActiveVatRateAsync(default)).ReturnsAsync(vatRate);
    }

    // ── Not found ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_OrderNotFound_ReturnsFailure()
    {
        _orderRepoMock.Setup(r => r.GetByIdWithItemsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((SalesOrder?)null);
        _taxMock.Setup(t => t.GetActiveVatRateAsync(default)).ReturnsAsync(0m);

        var result = await CreateHandler().Handle(
            new CompleteOrderCommand(Guid.NewGuid(), 100_000m), default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Không tìm thấy"));
    }

    [Fact]
    public async Task Handle_ProductNotFound_ReturnsFailure()
    {
        var order = DraftOrderWithItem(out _, out _);
        _orderRepoMock.Setup(r => r.GetByIdWithItemsAsync(It.IsAny<Guid>(), default)).ReturnsAsync(order);
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Product?)null);
        _taxMock.Setup(t => t.GetActiveVatRateAsync(default)).ReturnsAsync(0m);

        var result = await CreateHandler().Handle(new CompleteOrderCommand(order.Id, 100_000m), default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("không còn tồn tại"));
    }

    [Fact]
    public async Task Handle_VariantNotFound_ReturnsFailure()
    {
        var order = DraftOrderWithItem(out _, out _);
        _orderRepoMock.Setup(r => r.GetByIdWithItemsAsync(It.IsAny<Guid>(), default)).ReturnsAsync(order);
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync(BuildProduct());
        _variantRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync((ProductVariant?)null);
        _taxMock.Setup(t => t.GetActiveVatRateAsync(default)).ReturnsAsync(0m);

        var result = await CreateHandler().Handle(new CompleteOrderCommand(order.Id, 100_000m), default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Biến thể") && e.Contains("không còn tồn tại"));
    }

    // ── Tax-inclusive: TotalAmount never changes ───────────────────────────────

    [Fact]
    public async Task Handle_VatEnabled_TotalAmountUnchanged()
    {
        // SubTotal = 30,000 (tax-inclusive at 1% VAT)
        // TotalAmount must stay 30,000 — customer does NOT pay extra
        var order = DraftOrderWithItem(out var productId, out var variantId);
        SetupHappyPath(productId, variantId, order, 0.01m);

        var result = await CreateHandler().Handle(
            new CompleteOrderCommand(order.Id, 30_000m), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalAmount.Should().Be(30_000m); // unchanged
    }

    [Fact]
    public async Task Handle_VatEnabled_TaxExtractedFromTotal()
    {
        // SubTotal = 30,000, VAT 1%
        // Expected extracted tax = 30,000 * 0.01 / 1.01 ≈ 297.03
        var order = DraftOrderWithItem(out var productId, out var variantId);
        SetupHappyPath(productId, variantId, order, 0.01m);

        var result = await CreateHandler().Handle(
            new CompleteOrderCommand(order.Id, 30_000m), default);

        result.IsSuccess.Should().BeTrue();
        var expectedTax = Math.Round(30_000m * 0.01m / 1.01m, 2, MidpointRounding.AwayFromZero);
        result.Value!.TaxAmount.Should().Be(expectedTax);
    }

    [Fact]
    public async Task Handle_VatDisabled_TaxAmountIsZero()
    {
        var order = DraftOrderWithItem(out var productId, out var variantId);
        SetupHappyPath(productId, variantId, order, 0m);

        var result = await CreateHandler().Handle(
            new CompleteOrderCommand(order.Id, 30_000m), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TaxAmount.Should().Be(0m);
        result.Value.TotalAmount.Should().Be(30_000m);
    }

    // ── Edge cases ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithDiscount_TaxExtractedFromNetTotal()
    {
        // SubTotal = 30,000, Discount = 5,000 → taxable = 25,000
        // Tax = 25,000 * 0.01 / 1.01 ≈ 247.52
        var order = DraftOrderWithItem(out var productId, out var variantId);
        order.ApplyDiscount(5_000m);
        SetupHappyPath(productId, variantId, order, 0.01m);

        var result = await CreateHandler().Handle(
            new CompleteOrderCommand(order.Id, 25_000m), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalAmount.Should().Be(25_000m); // discount applied, no tax added
        var expectedTax = Math.Round(25_000m * 0.01m / 1.01m, 2, MidpointRounding.AwayFromZero);
        result.Value.TaxAmount.Should().Be(expectedTax);
    }

    // ── Already completed ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_AlreadyCompletedOrder_ReturnsFailure()
    {
        var order = DraftOrderWithItem(out var productId, out var variantId);
        order.Complete(30_000m);

        _orderRepoMock.Setup(r => r.GetByIdWithItemsAsync(It.IsAny<Guid>(), default)).ReturnsAsync(order);
        _productRepoMock.Setup(r => r.GetByIdAsync(productId, default)).ReturnsAsync(BuildProduct(100));
        _variantRepoMock.Setup(r => r.GetByIdAsync(productId, variantId, default))
            .ReturnsAsync(BuildVariant(productId));
        _taxMock.Setup(t => t.GetActiveVatRateAsync(default)).ReturnsAsync(0.01m);

        var result = await CreateHandler().Handle(
            new CompleteOrderCommand(order.Id, 40_000m), default);

        result.IsSuccess.Should().BeFalse();
    }
}