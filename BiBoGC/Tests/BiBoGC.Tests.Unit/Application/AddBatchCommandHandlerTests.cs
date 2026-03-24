using FluentAssertions;
using InventoryManagement.Application.Commands.AddBatch;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.ValueObjects;
using Moq;

namespace BiBoGC.Tests.Unit.Application;

public class AddBatchCommandHandlerTests
{
    private readonly Mock<IProductBatchRepository> _batchRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();

    private AddBatchCommandHandler CreateHandler()
    {
        return new AddBatchCommandHandler(_productRepoMock.Object, _batchRepoMock.Object);
    }

    private static Product BuildProduct()
    {
        return new Product("Sữa tươi", "Mô tả", ProductStatuses.Active,
            new Sku("SKU-MILK"), true, Units.Hop,
            new Money(25_000m), null);
    }

    private static AddBatchCommand ValidCommand(Guid productId)
    {
        return new AddBatchCommand
        {
            ProductId = productId,
            BatchNumber = "LOT-2026-001",
            Quantity = 200,
            ManufacturingDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ExpirationDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
            CostPrice = 20_000m
        };
    }

    // ── Product not found ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ProductNotFound_ReturnsFailure()
    {
        _productRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Product?)null);

        var result = await CreateHandler().Handle(ValidCommand(Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Không tìm thấy sản phẩm"));
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidCommand_ReturnsBatchDto()
    {
        var product = BuildProduct();
        var batchToReturn = new ProductBatch(product.Id, "LOT-2026-001", 200,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc), 20_000m);

        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);
        _batchRepoMock.Setup(r => r.AddAsync(It.IsAny<ProductBatch>(), default)).ReturnsAsync(batchToReturn);

        var result = await CreateHandler().Handle(ValidCommand(product.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.BatchNumber.Should().Be("LOT-2026-001");
        result.Value.Quantity.Should().Be(200);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsBatchRepositoryAdd()
    {
        var product = BuildProduct();
        var batchToReturn = new ProductBatch(product.Id, "LOT-2026-001", 200,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc));

        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);
        _batchRepoMock.Setup(r => r.AddAsync(It.IsAny<ProductBatch>(), default)).ReturnsAsync(batchToReturn);

        await CreateHandler().Handle(ValidCommand(product.Id), default);

        _batchRepoMock.Verify(r => r.AddAsync(It.IsAny<ProductBatch>(), default), Times.Once);
    }

    // ── Invalid batch (expiry before manufacture) handled gracefully ──────────

    [Fact]
    public async Task Handle_ExpiryBeforeManufacture_ReturnsFailure()
    {
        var product = BuildProduct();
        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        var cmd = ValidCommand(product.Id) with
        {
            ManufacturingDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
            ExpirationDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) // before manufacture
        };

        var result = await CreateHandler().Handle(cmd, default);

        result.IsSuccess.Should().BeFalse();
    }
}