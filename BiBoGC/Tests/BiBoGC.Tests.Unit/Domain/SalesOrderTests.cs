using FluentAssertions;
using Sale.Domain.Domain;
using Sale.Domain.Enum;
using Sale.Domain.Exceptions;

namespace BiBoGC.Tests.Unit.Domain;

public class SalesOrderTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static SalesOrder CreateDraftOrder()
    {
        return new SalesOrder("ORD-001", PaymentMethod.Cash);
    }

    private static void AddItem(SalesOrder order, decimal unitPrice = 10_000m, int qty = 1)
    {
        order.AddItem(Guid.NewGuid(), Guid.NewGuid(), null,
            "Sản phẩm A", "Loại 1", "SKU-001", "cái", qty, unitPrice);
    }

    // ── Construction ─────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithValidData_CreatesDraftOrder()
    {
        var order = new SalesOrder("ORD-001", PaymentMethod.Cash, "Khách A", "0901234567");

        order.Status.Should().Be(OrderStatus.Draft);
        order.OrderNumber.Should().Be("ORD-001");
        order.SubTotal.Should().Be(0);
        order.TaxAmount.Should().Be(0);
        order.TotalAmount.Should().Be(0);
        order.Items.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithBlankOrderNumber_ThrowsArgumentException(string orderNumber)
    {
        var act = () => new SalesOrder(orderNumber, PaymentMethod.Cash);

        act.Should().Throw<ArgumentException>().WithParameterName("orderNumber");
    }

    // ── AddItem ───────────────────────────────────────────────────────────────

    [Fact]
    public void AddItem_SingleItem_CalculatesSubTotalCorrectly()
    {
        var order = CreateDraftOrder();

        order.AddItem(Guid.NewGuid(), Guid.NewGuid(), null,
            "Nước ngọt", "Chai 500ml", "SKU-A", "chai", 3, 15_000m);

        order.SubTotal.Should().Be(45_000m);
        order.TotalAmount.Should().Be(45_000m);
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void AddItem_MultipleItems_SumsAllLineTotals()
    {
        var order = CreateDraftOrder();
        var pid1 = Guid.NewGuid();
        var vid1 = Guid.NewGuid();
        var pid2 = Guid.NewGuid();
        var vid2 = Guid.NewGuid();

        order.AddItem(pid1, vid1, null, "A", "A1", "SKU-1", "cái", 2, 10_000m); // 20,000
        order.AddItem(pid2, vid2, null, "B", "B1", "SKU-2", "cái", 3, 5_000m); // 15,000

        order.SubTotal.Should().Be(35_000m);
        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public void AddItem_SameVariant_IncrementsQuantityInsteadOfAddingNewLine()
    {
        var order = CreateDraftOrder();
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        order.AddItem(productId, variantId, null, "A", "A1", "SKU-1", "cái", 2, 10_000m);
        order.AddItem(productId, variantId, null, "A", "A1", "SKU-1", "cái", 3, 10_000m);

        order.Items.Should().HaveCount(1);
        order.Items.First().Quantity.Should().Be(5);
        order.SubTotal.Should().Be(50_000m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddItem_WithZeroOrNegativeQuantity_ThrowsArgumentException(int qty)
    {
        var order = CreateDraftOrder();

        var act = () => order.AddItem(Guid.NewGuid(), Guid.NewGuid(), null,
            "A", "A1", "SKU-1", "cái", qty, 10_000m);

        act.Should().Throw<ArgumentException>().WithParameterName("quantity");
    }

    [Fact]
    public void AddItem_WithNegativeUnitPrice_ThrowsArgumentException()
    {
        var order = CreateDraftOrder();

        var act = () => order.AddItem(Guid.NewGuid(), Guid.NewGuid(), null,
            "A", "A1", "SKU-1", "cái", 1, -1m);

        act.Should().Throw<ArgumentException>().WithParameterName("unitPrice");
    }

    [Fact]
    public void AddItem_OnCompletedOrder_ThrowsInvalidOrderStateException()
    {
        var order = CreateDraftOrder();
        AddItem(order, 10_000m);
        order.Complete(10_000m);

        var act = () => AddItem(order);

        act.Should().Throw<InvalidOrderStateException>();
    }

    [Fact]
    public void AddItem_OnCancelledOrder_ThrowsInvalidOrderStateException()
    {
        var order = CreateDraftOrder();
        order.Cancel();

        var act = () => AddItem(order);

        act.Should().Throw<InvalidOrderStateException>();
    }

    // ── RemoveItem ────────────────────────────────────────────────────────────

    [Fact]
    public void RemoveItem_ExistingItem_SubTotalRecalculated()
    {
        var order = CreateDraftOrder();
        var pid = Guid.NewGuid();
        var vid = Guid.NewGuid();
        order.AddItem(pid, vid, null, "A", "A1", "SKU-1", "cái", 2, 10_000m);
        var itemId = order.Items.First().Id;

        order.RemoveItem(itemId);

        order.Items.Should().BeEmpty();
        order.SubTotal.Should().Be(0);
        order.TotalAmount.Should().Be(0);
    }

    [Fact]
    public void RemoveItem_NonExistentId_DoesNothing()
    {
        var order = CreateDraftOrder();
        AddItem(order, 10_000m);

        var act = () => order.RemoveItem(Guid.NewGuid());

        act.Should().NotThrow();
        order.Items.Should().HaveCount(1);
    }

    // ── ApplyDiscount ─────────────────────────────────────────────────────────

    [Fact]
    public void ApplyDiscount_ValidAmount_ReducesTotalAmount()
    {
        var order = CreateDraftOrder();
        AddItem(order, 100_000m);

        order.ApplyDiscount(10_000m);

        order.DiscountAmount.Should().Be(10_000m);
        order.TotalAmount.Should().Be(90_000m);
    }

    [Fact]
    public void ApplyDiscount_EqualToSubTotal_TotalAmountIsZero()
    {
        var order = CreateDraftOrder();
        AddItem(order, 100_000m);

        order.ApplyDiscount(100_000m);

        order.TotalAmount.Should().Be(0);
    }

    [Fact]
    public void ApplyDiscount_GreaterThanSubTotal_ThrowsArgumentException()
    {
        var order = CreateDraftOrder();
        AddItem(order, 100_000m);

        var act = () => order.ApplyDiscount(100_001m);

        act.Should().Throw<ArgumentException>().WithParameterName("amount");
    }

    [Fact]
    public void ApplyDiscount_NegativeAmount_ThrowsArgumentException()
    {
        var order = CreateDraftOrder();
        AddItem(order, 100_000m);

        var act = () => order.ApplyDiscount(-1m);

        act.Should().Throw<ArgumentException>().WithParameterName("amount");
    }

    // ── ApplyTax (tax-inclusive extraction) ──────────────────────────────────
    // Prices INCLUDE tax. ApplyTax extracts the embedded tax for reporting.
    // TotalAmount NEVER changes — customer pays the same amount.
    // Formula: taxAmount = (SubTotal - Discount) * rate / (1 + rate)

    [Fact]
    public void ApplyTax_OnePercent_ExtractsTaxWithoutChangingTotal()
    {
        var order = CreateDraftOrder();
        AddItem(order, 100_000m); // tax-inclusive price

        order.ApplyTax(0.01m);

        // tax = 100,000 * 0.01 / 1.01 ≈ 990.10
        var expectedTax = Math.Round(100_000m * 0.01m / 1.01m, 2, MidpointRounding.AwayFromZero);
        order.TaxAmount.Should().Be(expectedTax);
        order.TotalAmount.Should().Be(100_000m); // unchanged — customer pays the same
    }

    [Fact]
    public void ApplyTax_ZeroRate_TaxAmountIsZero()
    {
        var order = CreateDraftOrder();
        AddItem(order, 100_000m);

        order.ApplyTax(0m);

        order.TaxAmount.Should().Be(0);
        order.TotalAmount.Should().Be(100_000m); // unchanged
    }

    [Fact]
    public void ApplyTax_WithDiscount_TaxExtractedFromNetAmount()
    {
        // taxable = SubTotal - Discount = 100,000 - 10,000 = 90,000
        // tax = 90,000 * 0.01 / 1.01 ≈ 891.09
        var order = CreateDraftOrder();
        AddItem(order, 100_000m);
        order.ApplyDiscount(10_000m);

        order.ApplyTax(0.01m);

        var expectedTax = Math.Round(90_000m * 0.01m / 1.01m, 2, MidpointRounding.AwayFromZero);
        order.TaxAmount.Should().Be(expectedTax);
        order.TotalAmount.Should().Be(90_000m); // unchanged — discount applied, no extra tax
    }

    [Fact]
    public void ApplyTax_ThenAddItem_TaxResetToZero()
    {
        // RecalculateTotals resets TaxAmount so it must be re-applied before Complete
        var order = CreateDraftOrder();
        AddItem(order, 100_000m);
        order.ApplyTax(0.01m);

        AddItem(order, 50_000m); // triggers RecalculateTotals

        order.TaxAmount.Should().Be(0);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void ApplyTax_OutOfRange_ThrowsArgumentException(double rate)
    {
        var order = CreateDraftOrder();
        AddItem(order, 100_000m);

        var act = () => order.ApplyTax((decimal)rate);

        act.Should().Throw<ArgumentException>().WithParameterName("taxRate");
    }

    [Fact]
    public void ApplyTax_OnCompletedOrder_ThrowsInvalidOrderStateException()
    {
        var order = CreateDraftOrder();
        AddItem(order, 10_000m);
        order.Complete(10_000m);

        var act = () => order.ApplyTax(0.01m);

        act.Should().Throw<InvalidOrderStateException>();
    }

    // Extraction rounds to 2 decimal places (AwayFromZero).
    [Theory]
    [InlineData(101_010, 0.01, 1_000.10)] // 101010 * 0.01/1.01 = 1000.099... → 1000.10
    [InlineData(100_000, 0.01, 990.10)] // 100000 * 0.01/1.01 = 990.099...  → 990.10
    [InlineData(10_100, 0.01, 100.00)] // 10100  * 0.01/1.01 = 100.000      → 100.00
    [InlineData(0, 0.01, 0.00)] // zero total → zero tax
    [InlineData(100_000, 0.00, 0.00)] // zero rate  → zero tax
    public void ApplyTax_ExtractionFormula_RoundsCorrectly(
        decimal total, decimal rate, decimal expectedTax)
    {
        var order = CreateDraftOrder();
        if (total > 0)
            order.AddItem(Guid.NewGuid(), Guid.NewGuid(), null,
                "X", "X1", "SKU-X", "cái", 1, total);

        order.ApplyTax(rate);

        order.TaxAmount.Should().Be(expectedTax);
        order.TotalAmount.Should().Be(total); // ALWAYS unchanged
    }

    // ── Complete ──────────────────────────────────────────────────────────────

    [Fact]
    public void Complete_WithSufficientPayment_StatusBecomesCompleted()
    {
        var order = CreateDraftOrder();
        AddItem(order, 100_000m);

        order.Complete(100_000m);

        order.Status.Should().Be(OrderStatus.Completed);
        order.AmountPaid.Should().Be(100_000m);
        order.ChangeAmount.Should().Be(0);
    }

    [Fact]
    public void Complete_WithOverpayment_ChangeAmountIsPositive()
    {
        var order = CreateDraftOrder();
        AddItem(order, 100_000m);

        order.Complete(150_000m);

        order.ChangeAmount.Should().Be(50_000m);
    }

    [Fact]
    public void Complete_WithInsufficientPayment_ThrowsInvalidOperationException()
    {
        var order = CreateDraftOrder();
        AddItem(order, 100_000m);

        var act = () => order.Complete(99_999m);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*99*999*");
    }

    [Fact]
    public void Complete_EmptyOrder_ThrowsInvalidOperationException()
    {
        var order = CreateDraftOrder();

        var act = () => order.Complete(0m);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*trống*");
    }

    [Fact]
    public void Complete_AlreadyCompleted_ThrowsInvalidOrderStateException()
    {
        var order = CreateDraftOrder();
        AddItem(order, 10_000m);
        order.Complete(10_000m);

        var act = () => order.Complete(10_000m);

        act.Should().Throw<InvalidOrderStateException>();
    }

    // ── Cancel ────────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_DraftOrder_StatusBecomesCancelled()
    {
        var order = CreateDraftOrder();

        order.Cancel("hết hàng");

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.Notes.Should().Contain("hết hàng");
    }

    [Fact]
    public void Cancel_AlreadyCancelled_ThrowsInvalidOperationException()
    {
        var order = CreateDraftOrder();
        order.Cancel();

        var act = () => order.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_CompletedOrder_ThrowsInvalidOperationException()
    {
        var order = CreateDraftOrder();
        AddItem(order, 10_000m);
        order.Complete(10_000m);

        var act = () => order.Cancel();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*hoàn thành*");
    }

    // ── SetInvoiceId ──────────────────────────────────────────────────────────

    [Fact]
    public void SetInvoiceId_OnCompletedOrder_SetsInvoiceId()
    {
        var order = CreateDraftOrder();
        AddItem(order, 10_000m);
        order.Complete(10_000m);
        var invoiceId = Guid.NewGuid();

        order.SetInvoiceId(invoiceId);

        order.InvoiceId.Should().Be(invoiceId);
    }

    [Fact]
    public void SetInvoiceId_OnDraftOrder_ThrowsInvalidOperationException()
    {
        var order = CreateDraftOrder();

        var act = () => order.SetInvoiceId(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SetInvoiceId_WhenAlreadyLinked_ThrowsInvalidOperationException()
    {
        var order = CreateDraftOrder();
        AddItem(order, 10_000m);
        order.Complete(10_000m);
        order.SetInvoiceId(Guid.NewGuid());

        var act = () => order.SetInvoiceId(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*đã có hóa đơn*");
    }

    // ── Domain events ─────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_RaisesSalesOrderCreatedEvent()
    {
        var order = new SalesOrder("ORD-001", PaymentMethod.Cash);

        order.DomainEvents.Should().ContainSingle(e =>
            e.GetType().Name == "SalesOrderCreatedEvent");
    }

    [Fact]
    public void Complete_RaisesSalesOrderCompletedEvent()
    {
        var order = CreateDraftOrder();
        AddItem(order, 10_000m);
        order.ClearDomainEvents();

        order.Complete(10_000m);

        order.DomainEvents.Should().ContainSingle(e =>
            e.GetType().Name == "SalesOrderCompletedEvent");
    }
}