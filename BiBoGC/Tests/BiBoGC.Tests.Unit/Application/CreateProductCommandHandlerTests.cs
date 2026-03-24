using FluentAssertions;
using InventoryManagement.Application.Commands.CreateProduct;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using Moq;

namespace BiBoGC.Tests.Unit.Application;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _repoMock = new();

    private CreateProductCommandHandler CreateHandler()
    {
        return new CreateProductCommandHandler(_repoMock.Object);
    }

    private static CreateProductCommand ValidCommand(string sku = "SKU-001")
    {
        return new CreateProductCommand
        {
            Name = "Coca Cola",
            Sku = sku,
            Price = 12_000m,
            BaseUnits = Units.Lon,
            Description = "Nước ngọt",
            RequiresBatchTracking = false,
            LowStockThreshold = 0
        };
    }

    // ── SKU duplicate guard ───────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DuplicateSku_ReturnsFailure()
    {
        _repoMock
            .Setup(r => r.SkuExistsAsync("SKU-DUP", null, default))
            .ReturnsAsync(true);

        var result = await CreateHandler().Handle(ValidCommand("SKU-DUP"), default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("SKU-DUP"));
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NewSku_CreatesProductSuccessfully()
    {
        _repoMock.Setup(r => r.SkuExistsAsync(It.IsAny<string>(), null, default)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>(), default))
            .ReturnsAsync((Product p, CancellationToken _) => p);

        var result = await CreateHandler().Handle(ValidCommand(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Sku.Should().Be("SKU-001");
        result.Value.Name.Should().Be("Coca Cola");
    }

    [Fact]
    public async Task Handle_NewProduct_HasActiveStatus()
    {
        _repoMock.Setup(r => r.SkuExistsAsync(It.IsAny<string>(), null, default)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>(), default))
            .ReturnsAsync((Product p, CancellationToken _) => p);

        var result = await CreateHandler().Handle(ValidCommand(), default);

        result.Value!.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_NewProduct_DefaultVariantAutoCreated()
    {
        _repoMock.Setup(r => r.SkuExistsAsync(It.IsAny<string>(), null, default)).ReturnsAsync(false);

        Product? saved = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>(), default))
            .Callback<Product, CancellationToken>((p, _) => saved = p)
            .ReturnsAsync((Product p, CancellationToken _) => p);

        await CreateHandler().Handle(ValidCommand(), default);

        saved!.Variants.Should().ContainSingle(v => v.SkuUnique.Value.Contains("DEFAULT"));
    }

    [Fact]
    public async Task Handle_NewProduct_CallsRepositoryAdd()
    {
        _repoMock.Setup(r => r.SkuExistsAsync(It.IsAny<string>(), null, default)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>(), default))
            .ReturnsAsync((Product p, CancellationToken _) => p);

        await CreateHandler().Handle(ValidCommand(), default);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Product>(), default), Times.Once);
    }
}