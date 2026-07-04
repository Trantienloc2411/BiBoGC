using FluentAssertions;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.ValueObjects;

namespace BiBoGC.Tests.Unit.Domain;

public class ProductTests
{
    private static Product BuildProduct(bool requiresBatch = false)
    {
        return new Product("Nước ngọt", "Mô tả", ProductStatuses.Active,
            new Sku("SKU-001"), requiresBatch, Units.Lon,
            null);
    }

    // ── IncreaseStock / DecreaseStock ─────────────────────────────────────────

    [Fact]
    public void IncreaseStock_PositiveAmount_UpdatesTotalStock()
    {
        var product = BuildProduct();
        product.IncreaseStock(50);
        product.TotalStock.Should().Be(50);
    }

    [Fact]
    public void IncreaseStock_Twice_Accumulates()
    {
        var product = BuildProduct();
        product.IncreaseStock(30);
        product.IncreaseStock(20);
        product.TotalStock.Should().Be(50);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IncreaseStock_ZeroOrNegative_Throws(int qty)
    {
        var product = BuildProduct();
        var act = () => product.IncreaseStock(qty);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DecreaseStock_SufficientStock_Reduces()
    {
        var product = BuildProduct();
        product.IncreaseStock(100);
        product.DecreaseStock(30);
        product.TotalStock.Should().Be(70);
    }

    [Fact]
    public void DecreaseStock_InsufficientStock_Throws()
    {
        var product = BuildProduct();
        product.IncreaseStock(5);
        var act = () => product.DecreaseStock(10);
        act.Should().Throw<InvalidOperationException>();
    }

    // ── GetAvailableStock (batch-based) ───────────────────────────────────────

    [Fact]
    public void GetAvailableStock_NoBatches_ReturnsZero()
    {
        var product = BuildProduct();
        product.GetAvailableStock().Should().Be(0);
    }

    [Fact]
    public void GetAvailableStock_WithNonExpiredBatch_ReturnsBatchQty()
    {
        var product = BuildProduct();
        product.AddNewBatch("LOT-A", 100, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(90));
        product.GetAvailableStock().Should().Be(100);
    }

    [Fact]
    public void GetAvailableStock_ExpiredBatchesExcluded()
    {
        var product = BuildProduct();
        product.AddNewBatch("LOT-OLD", 50, DateTime.UtcNow.AddDays(-200), DateTime.UtcNow.AddDays(-1));
        product.AddNewBatch("LOT-NEW", 80, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(60));
        product.GetAvailableStock().Should().Be(80);
    }

    // ── IsLowStock ─────────────────────────────────────────────────────────────

    [Fact]
    public void IsLowStock_NoThreshold_ReturnsFalse()
    {
        var product = new Product("P", "", ProductStatuses.Active,
            new Sku("S1"), false, Units.Pcs, null);
        product.IsLowStock().Should().BeFalse();
    }

    [Fact]
    public void IsLowStock_StockBelowThreshold_ReturnsTrue()
    {
        var product = new Product("P", "", ProductStatuses.Active,
            new Sku("S2"), false, Units.Pcs, 10);
        product.IncreaseStock(5);
        product.IsLowStock().Should().BeTrue();
    }

    [Fact]
    public void IsLowStock_StockAboveThreshold_ReturnsFalse()
    {
        var product = new Product("P", "", ProductStatuses.Active,
            new Sku("S3"), false, Units.Pcs, 10);
        product.IncreaseStock(15);
        product.IsLowStock().Should().BeFalse();
    }

    // ── AddNewBatch duplicate guard ───────────────────────────────────────────

    [Fact]
    public void AddNewBatch_DuplicateBatchNumber_Throws()
    {
        var product = BuildProduct();
        product.AddNewBatch("LOT-A", 10, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(30));
        var act = () => product.AddNewBatch("LOT-A", 20, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(30));
        act.Should().Throw<InvalidOperationException>();
    }

    // ── Discontinue ───────────────────────────────────────────────────────────

    [Fact]
    public void Discontinue_ActiveProduct_ChangesStatusToDiscontinued()
    {
        var product = BuildProduct();
        product.Discontinue();
        product.Status.Should().Be(ProductStatuses.Discontinued);
    }

    // ── GetExpiredBatches ─────────────────────────────────────────────────────

    [Fact]
    public void GetExpiredBatches_ReturnsOnlyExpired()
    {
        var product = BuildProduct();
        product.AddNewBatch("OLD", 20, DateTime.UtcNow.AddDays(-100), DateTime.UtcNow.AddDays(-1));
        product.AddNewBatch("NEW", 50, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(60));

        var expired = product.GetExpiredBatches().ToList();

        expired.Should().ContainSingle(b => b.BatchNumber == "OLD");
    }
}