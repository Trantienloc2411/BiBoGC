using FluentAssertions;
using Sale.Domain.Domain;
using Sale.Domain.Enum;

namespace BiBoGC.Tests.Unit.Helpers;

/// <summary>
/// Tax calculation correctness tests.
///
/// Business rule: TAX-INCLUSIVE pricing.
///   - Product prices already include VAT.
///   - Customer pays exactly the listed price — no extra tax at checkout.
///   - ApplyTax() EXTRACTS the embedded tax for accounting/reporting.
///   - TotalAmount is NEVER changed by ApplyTax().
///
/// Extraction formula: taxAmount = total * rate / (1 + rate)
/// </summary>
public class TaxCalculationTests
{
    private static SalesOrder OrderWithSubTotal(decimal subTotal)
    {
        var order = new SalesOrder("ORD-TAX", PaymentMethod.Cash);
        if (subTotal > 0)
            order.AddItem(Guid.NewGuid(), Guid.NewGuid(), null,
                "Item", "V1", "SKU", "cái", 1, subTotal);
        return order;
    }

    // ── VAT 1% extraction at order level ─────────────────────────────────────

    [Theory]
    [InlineData(100_000, 990.10)] // 100000 × 0.01/1.01 = 990.099… → 990.10
    [InlineData(1_000_000, 9_900.99)] // 1000000 × 0.01/1.01 = 9900.990… → 9900.99
    [InlineData(0, 0)] // zero total → zero tax
    [InlineData(1, 0.01)] // 1 × 0.01/1.01 = 0.0099… → 0.01
    [InlineData(100_001, 990.11)] // 100001 × 0.01/1.01 = 990.108… → 990.11
    [InlineData(100_050, 990.59)] // 100050 × 0.01/1.01 = 990.594… → 990.59
    public void ApplyTax_VatOnePercent_ExtractsTaxAtTwoDp(decimal subTotal, decimal expectedTax)
    {
        var order = OrderWithSubTotal(subTotal);

        order.ApplyTax(0.01m);

        order.TaxAmount.Should().Be(expectedTax,
            $"extracted tax of {subTotal} @ 1% should be {expectedTax}");
        order.TotalAmount.Should().Be(subTotal,
            "TotalAmount must not change — customer pays tax-inclusive price");
    }

    // ── PIT 0.5% extraction at order level ───────────────────────────────────

    [Theory]
    [InlineData(100_000, 497.51)] // 100000 × 0.005/1.005 = 497.512… → 497.51
    [InlineData(200_000, 995.02)] // 200000 × 0.005/1.005 = 995.024… → 995.02
    [InlineData(1_000, 4.98)] // 1000   × 0.005/1.005 = 4.9751… → 4.98
    [InlineData(100, 0.50)] // 100    × 0.005/1.005 = 0.4975… → 0.50
    [InlineData(1, 0.00)] // 1      × 0.005/1.005 = 0.00497… → 0.00
    public void ApplyTax_PitHalfPercent_ExtractsTaxAtTwoDp(decimal subTotal, decimal expectedTax)
    {
        var order = OrderWithSubTotal(subTotal);

        order.ApplyTax(0.005m);

        order.TaxAmount.Should().Be(expectedTax,
            $"extracted tax of {subTotal} @ 0.5% should be {expectedTax}");
        order.TotalAmount.Should().Be(subTotal,
            "TotalAmount must not change");
    }

    // ── Discount then tax ─────────────────────────────────────────────────────

    [Fact]
    public void TotalAmount_WithDiscountAndTax_TotalUnchanged()
    {
        // SubTotal = 1,000,000; Discount = 100,000 → TotalAmount = 900,000
        // Tax is extracted from 900,000: 900,000 × 0.01/1.01 = 8,910.891… → 8,910.89
        var order = OrderWithSubTotal(1_000_000m);
        order.ApplyDiscount(100_000m);
        order.ApplyTax(0.01m);

        order.TotalAmount.Should().Be(900_000m, "discount is applied, but tax does not add to total");
        order.TaxAmount.Should().Be(Math.Round(900_000m * 0.01m / 1.01m, 2, MidpointRounding.AwayFromZero));
    }

    [Fact]
    public void TotalAmount_NeverNegative_WhenDiscountEqualsSubTotal()
    {
        var order = OrderWithSubTotal(100_000m);
        order.ApplyDiscount(100_000m);

        order.TotalAmount.Should().Be(0);
        order.TaxAmount.Should().Be(0); // TaxAmount reset by RecalculateTotals on zero total
    }

    // ── Tax declaration math (TT 40/2021 — flat rate on gross revenue) ───────
    // For Vietnamese household businesses, GTGT and TNCN are calculated as
    // rate × gross revenue (tax-inclusive). This is the legal formula under TT 40.

    [Theory]
    [InlineData(1_000_000, 10_000, 5_000)] // 1M revenue
    [InlineData(5_000_000, 50_000, 25_000)] // 5M revenue
    [InlineData(0, 0, 0)] // zero revenue
    [InlineData(1, 0, 0)] // 1đ rounds to 0
    [InlineData(200, 2, 1)] // 200 × 1% = 2, 200 × 0.5% = 1
    public void TaxDeclaration_GtgtAndTncn_FlatRateOnGrossRevenue(
        decimal revenue, decimal expectedGtgt, decimal expectedTncn)
    {
        // Mirrors ExportTaxDeclarationQueryHandler (TT 40 formula: rate × revenue)
        var gtgt = Math.Round(revenue * 0.01m, 0, MidpointRounding.AwayFromZero);
        var tncn = Math.Round(revenue * 0.005m, 0, MidpointRounding.AwayFromZero);

        gtgt.Should().Be(expectedGtgt, $"GTGT of {revenue} = {expectedGtgt}");
        tncn.Should().Be(expectedTncn, $"TNCN of {revenue} = {expectedTncn}");
    }

    // ── Large value: no overflow ──────────────────────────────────────────────

    [Fact]
    public void ApplyTax_VeryLargeSubTotal_DoesNotOverflow()
    {
        // 1 billion đ tax-inclusive, VAT 1%
        // extracted tax = 1,000,000,000 × 0.01 / 1.01 = 9,900,990.099… → 9,900,990.10
        var order = OrderWithSubTotal(1_000_000_000m);

        var act = () => order.ApplyTax(0.01m);

        act.Should().NotThrow();
        order.TaxAmount.Should().Be(9_900_990.10m);
        order.TotalAmount.Should().Be(1_000_000_000m); // unchanged
    }

    // ── Zero VAT rate ─────────────────────────────────────────────────────────

    [Fact]
    public void ApplyTax_ZeroRate_TaxIsZeroAndTotalUnchanged()
    {
        var order = OrderWithSubTotal(500_000m);

        order.ApplyTax(0m);

        order.TaxAmount.Should().Be(0);
        order.TotalAmount.Should().Be(500_000m);
    }
}