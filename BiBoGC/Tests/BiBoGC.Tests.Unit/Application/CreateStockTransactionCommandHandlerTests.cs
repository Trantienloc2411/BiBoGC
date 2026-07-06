using FluentAssertions;
using InventoryManagement.Application.Commands.CreateStockTransaction;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.ValueObjects;
using Moq;
using Shared.Application.Interfaces;

namespace BiBoGC.Tests.Unit.Application;

public class CreateStockTransactionCommandHandlerTests
{
    private readonly Mock<IAuditLogger> _auditMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<ISupplierRepository> _supplierRepoMock = new();
    private readonly Mock<IStockTransactionRepository> _txRepoMock = new();
    private readonly Mock<INotificationService> _notificationMock = new();

    private CreateStockTransactionCommandHandler CreateHandler()
    {
        return new CreateStockTransactionCommandHandler(_txRepoMock.Object, _productRepoMock.Object,
            _supplierRepoMock.Object, _auditMock.Object, _notificationMock.Object);
    }

    private static Product BuildProduct()
    {
        return new Product("Bia Hà Nội", "Mô tả", ProductStatuses.Active,
            new Sku("SKU-BIA"), false, Units.Lon, null);
    }

    private static StockTransaction BuildTxResult(Guid productId, Product product)
    {
        var tx = new StockTransaction(productId, Guid.Empty, Guid.Empty,
            StockTransactionType.Purchase, 100, 14_000m, DateTime.UtcNow, "nhập kho");
        // Set navigation property via reflection — handler needs Product.SkuGeneral in DTO mapping
        typeof(StockTransaction).GetProperty("Product")!.SetValue(tx, product);
        return tx;
    }

    // ── Product not found ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ProductNotFound_ReturnsFailure()
    {
        _productRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Product?)null);

        var result = await CreateHandler().Handle(new CreateStockTransactionCommand
        {
            ProductId = Guid.NewGuid(),
            TransactionType = StockTransactionType.Purchase,
            Quantity = 10,
            UnitPrice = 10_000m
        }, default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Không tìm thấy sản phẩm"));
    }

    // ── Supplier not found ────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_SupplierNotFound_ReturnsFailure()
    {
        var product = BuildProduct();
        var supplierId = Guid.NewGuid();

        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);
        _supplierRepoMock
            .Setup(r => r.GetByIdAsync(supplierId, default))
            .ReturnsAsync((Supplier?)null);

        var result = await CreateHandler().Handle(new CreateStockTransactionCommand
        {
            ProductId = product.Id,
            SupplierId = supplierId,
            TransactionType = StockTransactionType.Purchase,
            Quantity = 10,
            UnitPrice = 10_000m
        }, default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("nhà cung cấp"));
    }

    // ── Happy path: purchase without supplier ─────────────────────────────────

    [Fact]
    public async Task Handle_PurchaseWithoutSupplier_CreatesTransaction()
    {
        var product = BuildProduct();
        var txResult = BuildTxResult(product.Id, product);

        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);
        _txRepoMock.Setup(r => r.AddAsync(It.IsAny<StockTransaction>(), default)).ReturnsAsync(txResult);
        _txRepoMock.Setup(r => r.GetByIdAsync(txResult.Id, default)).ReturnsAsync(txResult);

        var result = await CreateHandler().Handle(new CreateStockTransactionCommand
        {
            ProductId = product.Id,
            TransactionType = StockTransactionType.Purchase,
            Quantity = 100,
            UnitPrice = 14_000m
        }, default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TransactionType.Should().Be("Purchase");
    }

    // ── Audit log written ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidTransaction_WritesAuditLog()
    {
        var product = BuildProduct();
        var txResult = BuildTxResult(product.Id, product);

        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);
        _txRepoMock.Setup(r => r.AddAsync(It.IsAny<StockTransaction>(), default)).ReturnsAsync(txResult);
        _txRepoMock.Setup(r => r.GetByIdAsync(txResult.Id, default)).ReturnsAsync(txResult);

        await CreateHandler().Handle(new CreateStockTransactionCommand
        {
            ProductId = product.Id,
            TransactionType = StockTransactionType.Purchase,
            Quantity = 50,
            UnitPrice = 12_000m
        }, default);

        _auditMock.Verify(a => a.LogAsync(
            "StockTransaction.Create", true,
            null, null,
            It.Is<string?>(d => d != null && d.Contains("Purchase")),
            default), Times.Once);
    }
}