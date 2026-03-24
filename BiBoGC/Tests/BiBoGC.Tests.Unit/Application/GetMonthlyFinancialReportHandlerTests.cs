using Finance.Application.Interfaces;
using Finance.Application.Queries.GetMonthlyFinancialReport;
using FluentAssertions;
using Moq;

namespace BiBoGC.Tests.Unit.Application;

public class GetMonthlyFinancialReportHandlerTests
{
    private readonly Mock<IExpenseRepository> _expenseMock = new();
    private readonly Mock<ISalesDataReader> _salesMock = new();
    private readonly Mock<IStockDataReader> _stockMock = new();

    private GetMonthlyFinancialReportQueryHandler CreateHandler()
    {
        return new GetMonthlyFinancialReportQueryHandler(_salesMock.Object, _stockMock.Object, _expenseMock.Object);
    }

    private void SetupData(decimal revenue, decimal cogs, decimal expenses)
    {
        _salesMock.Setup(r => r.GetAggregateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), default))
            .ReturnsAsync(new Finance.Application.ReadModels.SalesAggregate(revenue, 0));
        _stockMock.Setup(r => r.GetCogsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), default))
            .ReturnsAsync(cogs);
        _expenseMock.Setup(r => r.GetTotalAmountAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), default))
            .ReturnsAsync(expenses);
    }

    // ── Net profit calculation ────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithData_CalculatesNetProfit()
    {
        // Revenue 10M, COGS 4M, Expenses 2M → NetProfit = 4M
        SetupData(10_000_000m, 4_000_000m, 2_000_000m);

        var result = await CreateHandler().Handle(
            new GetMonthlyFinancialReportQuery(2026, 3), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.GrossProfit.Should().Be(6_000_000m);
        result.Value.NetProfit.Should().Be(4_000_000m);
    }

    // ── Profit margin percent ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithData_CalculatesProfitMarginPercent()
    {
        // Revenue 10M, Net 2M → 20%
        SetupData(10_000_000m, 5_000_000m, 3_000_000m);

        var result = await CreateHandler().Handle(
            new GetMonthlyFinancialReportQuery(2026, 3), default);

        result.Value!.ProfitMarginPercent.Should().Be(20m);
    }

    // ── Zero revenue guard ────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ZeroRevenue_MarginIsZeroNoDivisionByZero()
    {
        SetupData(0m, 0m, 0m);

        var result = await CreateHandler().Handle(
            new GetMonthlyFinancialReportQuery(2026, 3), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ProfitMarginPercent.Should().Be(0);
    }

    // ── Loss scenario ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CogsExceedsRevenue_NegativeNetProfit()
    {
        SetupData(1_000_000m, 800_000m, 500_000m);

        var result = await CreateHandler().Handle(
            new GetMonthlyFinancialReportQuery(2026, 3), default);

        result.Value!.NetProfit.Should().Be(-300_000m);
        result.Value.ProfitMarginPercent.Should().Be(-30m);
    }

    // ── Year/month in response ────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ReturnsCorrectYearMonth()
    {
        SetupData(0m, 0m, 0m);

        var result = await CreateHandler().Handle(
            new GetMonthlyFinancialReportQuery(2026, 6), default);

        result.Value!.Year.Should().Be(2026);
        result.Value.Month.Should().Be(6);
    }

    // ── Date range correctness ────────────────────────────────────────────────

    [Fact]
    public async Task Handle_PassesCorrectMonthRangeToReaders()
    {
        SetupData(0m, 0m, 0m);

        await CreateHandler().Handle(new GetMonthlyFinancialReportQuery(2026, 3), default);

        _salesMock.Verify(r => r.GetAggregateAsync(
            It.Is<DateTime>(d => d.Year == 2026 && d.Month == 3 && d.Day == 1),
            It.Is<DateTime>(d => d.Year == 2026 && d.Month == 3),
            default), Times.Once);
    }
}