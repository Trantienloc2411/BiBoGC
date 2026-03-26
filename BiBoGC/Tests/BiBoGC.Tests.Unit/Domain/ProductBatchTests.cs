using FluentAssertions;
using InventoryManagement.Domain.Entities;

namespace BiBoGC.Tests.Unit.Domain;

public class ProductBatchTests
{
    private static ProductBatch Build(int qty = 100)
    {
        return new ProductBatch(Guid.NewGuid(), "LOT-001", qty,
            DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(180));
    }

    // ── Constructor validation ─────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_ZeroOrNegativeQty_Throws(int qty)
    {
        var act = () => new ProductBatch(Guid.NewGuid(), "LOT-X", qty,
            DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(30));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ExpiryBeforeManufacture_Throws()
    {
        var act = () => new ProductBatch(Guid.NewGuid(), "LOT-X", 10,
            DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(1));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_BlankBatchNumber_Throws()
    {
        var act = () => new ProductBatch(Guid.NewGuid(), "  ", 10,
            DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(30));
        act.Should().Throw<ArgumentException>();
    }

    // ── IncreaseQuantity / DecreaseQuantity ───────────────────────────────────

    [Fact]
    public void IncreaseQuantity_AddsToExistingQty()
    {
        var batch = Build(50);
        batch.IncreaseQuantity(30);
        batch.Quantity.Should().Be(80);
    }

    [Fact]
    public void DecreaseQuantity_ReducesQty()
    {
        var batch = Build(50);
        batch.DecreaseQuantity(20);
        batch.Quantity.Should().Be(30);
    }

    [Fact]
    public void DecreaseQuantity_ExceedsStock_Throws()
    {
        var batch = Build(10);
        var act = () => batch.DecreaseQuantity(20);
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DecreaseQuantity_ZeroOrNegative_Throws(int qty)
    {
        var batch = Build(100);
        var act = () => batch.DecreaseQuantity(qty);
        act.Should().Throw<ArgumentException>();
    }

    // ── IsExpired ─────────────────────────────────────────────────────────────

    [Fact]
    public void IsExpired_FutureExpiry_ReturnsFalse()
    {
        var batch = new ProductBatch(Guid.NewGuid(), "LOT-F", 10,
            DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(5));
        batch.IsExpired().Should().BeFalse();
    }

    [Fact]
    public void IsExpired_PastExpiry_ReturnsTrue()
    {
        var batch = new ProductBatch(Guid.NewGuid(), "LOT-P", 10,
            DateTime.UtcNow.AddDays(-20), DateTime.UtcNow.AddDays(-1));
        batch.IsExpired().Should().BeTrue();
    }

    // ── IsExpiringSoon ────────────────────────────────────────────────────────

    [Fact]
    public void IsExpiringSoon_ExpiresWithin30Days_ReturnsTrue()
    {
        var batch = new ProductBatch(Guid.NewGuid(), "LOT-S", 10,
            DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(15));
        batch.IsExpiringSoon(30).Should().BeTrue();
    }

    [Fact]
    public void IsExpiringSoon_ExpiresAfter30Days_ReturnsFalse()
    {
        var batch = new ProductBatch(Guid.NewGuid(), "LOT-L", 10,
            DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(60));
        batch.IsExpiringSoon(30).Should().BeFalse();
    }
}