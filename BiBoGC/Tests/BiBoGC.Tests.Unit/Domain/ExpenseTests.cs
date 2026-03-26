using Finance.Domain.Entities;
using Finance.Domain.Enums;
using FluentAssertions;

namespace BiBoGC.Tests.Unit.Domain;

public class ExpenseTests
{
    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_CreatesExpense()
    {
        var date = new DateTime(2026, 3, 24, 10, 0, 0, DateTimeKind.Utc);

        var expense = Expense.Create(
            ExpenseCategory.Utilities,
            150_000m,
            "Tiền điện tháng 3",
            date,
            ExpensePaymentMethod.Cash);

        expense.Category.Should().Be(ExpenseCategory.Utilities);
        expense.Amount.Should().Be(150_000m);
        expense.Description.Should().Be("Tiền điện tháng 3");
        expense.PaymentMethod.Should().Be(ExpensePaymentMethod.Cash);
        expense.Id.Should().NotBe(Guid.Empty);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Create_WithZeroOrNegativeAmount_ThrowsArgumentException(decimal amount)
    {
        var act = () => Expense.Create(
            ExpenseCategory.Utilities, amount, "Mô tả",
            DateTime.UtcNow, ExpensePaymentMethod.Cash);

        act.Should().Throw<ArgumentException>().WithParameterName("amount");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankDescription_ThrowsArgumentException(string description)
    {
        var act = () => Expense.Create(
            ExpenseCategory.Utilities, 100m, description,
            DateTime.UtcNow, ExpensePaymentMethod.Cash);

        act.Should().Throw<ArgumentException>().WithParameterName("description");
    }

    [Fact]
    public void Create_TrimsDescription()
    {
        var expense = Expense.Create(
            ExpenseCategory.Utilities, 100m, "  Tiền thuê mặt bằng  ",
            DateTime.UtcNow, ExpensePaymentMethod.BankTransfer);

        expense.Description.Should().Be("Tiền thuê mặt bằng");
    }

    // ── DateTime UTC normalisation ────────────────────────────────────────────

    [Fact]
    public void Create_WithUtcDate_StoresAsUtc()
    {
        var utcDate = new DateTime(2026, 3, 24, 0, 0, 0, DateTimeKind.Utc);

        var expense = Expense.Create(
            ExpenseCategory.Others, 100m, "Test", utcDate, ExpensePaymentMethod.Cash);

        expense.ExpenseDate.Kind.Should().Be(DateTimeKind.Utc);
        expense.ExpenseDate.Should().Be(utcDate);
    }

    [Fact]
    public void Create_WithUnspecifiedKindDate_TreatsAsUtc()
    {
        var unspecified = new DateTime(2026, 3, 24, 0, 0, 0, DateTimeKind.Unspecified);

        var expense = Expense.Create(
            ExpenseCategory.Others, 100m, "Test", unspecified, ExpensePaymentMethod.Cash);

        expense.ExpenseDate.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Create_WithLocalDate_ConvertsToUtc()
    {
        var localDate = new DateTime(2026, 3, 24, 7, 0, 0, DateTimeKind.Local);

        var expense = Expense.Create(
            ExpenseCategory.Others, 100m, "Test", localDate, ExpensePaymentMethod.Cash);

        expense.ExpenseDate.Kind.Should().Be(DateTimeKind.Utc);
    }

    // ── Optional fields ───────────────────────────────────────────────────────

    [Fact]
    public void Create_WithNullReceiptNumber_StoresNull()
    {
        var expense = Expense.Create(
            ExpenseCategory.Supplies, 50_000m, "Mua văn phòng phẩm",
            DateTime.UtcNow, ExpensePaymentMethod.Cash, null);

        expense.ReceiptNumber.Should().BeNull();
    }

    [Fact]
    public void Create_WithReceiptNumber_TrimsAndStores()
    {
        var expense = Expense.Create(
            ExpenseCategory.Supplies, 50_000m, "Mua văn phòng phẩm",
            DateTime.UtcNow, ExpensePaymentMethod.Cash, "  HD-001  ");

        expense.ReceiptNumber.Should().Be("HD-001");
    }
}