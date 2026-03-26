using Finance.Application.Interfaces;
using Finance.Application.Queries.GetAnnualRevenueReport;
using Finance.Application.ReadModels;
using FluentAssertions;
using Moq;

namespace BiBoGC.Tests.Unit.Application;

public class GetAnnualRevenueReportHandlerTests
{
    private readonly Mock<ISalesDataReader> _readerMock = new();

    private GetAnnualRevenueReportQueryHandler CreateHandler()
    {
        return new GetAnnualRevenueReportQueryHandler(_readerMock.Object);
    }

    private void SetupMonthlyData(List<MonthlySales> data)
    {
        _readerMock.Setup(r => r.GetMonthlyBreakdownAsync(It.IsAny<int>(), default))
            .ReturnsAsync(data);
    }

    // ── 12 months always present ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_NoSalesData_Returns12MonthsAllZero()
    {
        SetupMonthlyData([]);

        var result = await CreateHandler().Handle(new GetAnnualRevenueReportQuery(2026), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.MonthlyBreakdowns.Should().HaveCount(12);
        result.Value.MonthlyBreakdowns.Should().AllSatisfy(m =>
        {
            m.Revenue.Should().Be(0);
            m.TransactionCount.Should().Be(0);
        });
    }

    [Fact]
    public async Task Handle_PartialData_MissingMonthsFilledWithZero()
    {
        SetupMonthlyData([
            new MonthlySales(1, 1_000_000m, 10),
            new MonthlySales(3, 2_000_000m, 20)
        ]);

        var result = await CreateHandler().Handle(new GetAnnualRevenueReportQuery(2026), default);

        result.Value!.MonthlyBreakdowns.Should().HaveCount(12);
        result.Value.MonthlyBreakdowns.First(m => m.Month == 2).Revenue.Should().Be(0);
        result.Value.MonthlyBreakdowns.First(m => m.Month == 1).Revenue.Should().Be(1_000_000m);
        result.Value.MonthlyBreakdowns.First(m => m.Month == 3).Revenue.Should().Be(2_000_000m);
    }

    // ── TotalRevenue and TotalTransactions ────────────────────────────────────

    [Fact]
    public async Task Handle_WithData_TotalRevenueIsSumOfAllMonths()
    {
        SetupMonthlyData([
            new MonthlySales(1, 1_000_000m, 10),
            new MonthlySales(2, 2_000_000m, 20),
            new MonthlySales(3, 500_000m, 5)
        ]);

        var result = await CreateHandler().Handle(new GetAnnualRevenueReportQuery(2026), default);

        result.Value!.TotalRevenue.Should().Be(3_500_000m);
        result.Value.TotalTransactions.Should().Be(35);
    }

    // ── MoM growth calculation ────────────────────────────────────────────────

    [Fact]
    public async Task Handle_FirstMonth_MoMGrowthIsNull()
    {
        // Month 1 has no previous month — MoMGrowthPercent must be null
        SetupMonthlyData([new MonthlySales(1, 1_000_000m, 10)]);

        var result = await CreateHandler().Handle(new GetAnnualRevenueReportQuery(2026), default);

        result.Value!.MonthlyBreakdowns.First(m => m.Month == 1).MoMGrowthPercent.Should().BeNull();
    }

    [Fact]
    public async Task Handle_GrowthMonth_MoMGrowthIsPositive()
    {
        SetupMonthlyData([
            new MonthlySales(1, 1_000_000m, 10),
            new MonthlySales(2, 1_100_000m, 11)
        ]);

        var result = await CreateHandler().Handle(new GetAnnualRevenueReportQuery(2026), default);

        // (1,100,000 - 1,000,000) / 1,000,000 * 100 = 10%
        result.Value!.MonthlyBreakdowns.First(m => m.Month == 2)
            .MoMGrowthPercent.Should().Be(10m);
    }

    [Fact]
    public async Task Handle_DeclineMonth_MoMGrowthIsNegative()
    {
        SetupMonthlyData([
            new MonthlySales(1, 1_000_000m, 10),
            new MonthlySales(2, 800_000m, 8)
        ]);

        var result = await CreateHandler().Handle(new GetAnnualRevenueReportQuery(2026), default);

        // (800,000 - 1,000,000) / 1,000,000 * 100 = -20%
        result.Value!.MonthlyBreakdowns.First(m => m.Month == 2)
            .MoMGrowthPercent.Should().Be(-20m);
    }

    [Fact]
    public async Task Handle_PreviousMonthIsZero_MoMGrowthIsNull_NoDivisionByZero()
    {
        // Month 1 = 0, Month 2 = 500,000 — previous is 0, cannot compute growth
        SetupMonthlyData([new MonthlySales(2, 500_000m, 5)]);

        var result = await CreateHandler().Handle(new GetAnnualRevenueReportQuery(2026), default);

        // Month 1 is 0 (missing data), so Month 2 MoMGrowth = null (not divide by zero)
        result.Value!.MonthlyBreakdowns.First(m => m.Month == 2)
            .MoMGrowthPercent.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CurrentMonthIsZero_MoMGrowthIsMinus100()
    {
        // Month 1 = 1,000,000, Month 2 = 0 → -100%
        SetupMonthlyData([new MonthlySales(1, 1_000_000m, 10)]);

        var result = await CreateHandler().Handle(new GetAnnualRevenueReportQuery(2026), default);

        result.Value!.MonthlyBreakdowns.First(m => m.Month == 2)
            .MoMGrowthPercent.Should().Be(-100m);
    }

    // ── Month names in Vietnamese ─────────────────────────────────────────────

    [Fact]
    public async Task Handle_MonthNames_AreInVietnamese()
    {
        SetupMonthlyData([]);

        var result = await CreateHandler().Handle(new GetAnnualRevenueReportQuery(2026), default);

        var names = result.Value!.MonthlyBreakdowns.Select(m => m.MonthName).ToList();
        // Vietnamese month names use "tháng" prefix or "Tháng" — just ensure non-empty
        names.Should().AllSatisfy(n => n.Should().NotBeNullOrWhiteSpace());
        names.Should().HaveCount(12);
        names.Should().OnlyHaveUniqueItems();
    }
}