using Finance.Application.Interfaces;
using Finance.Application.Queries.GetDailySalesReport;
using Finance.Application.ReadModels;
using FluentAssertions;
using Moq;

namespace BiBoGC.Tests.Unit.Application;

public class GetDailySalesReportHandlerTests
{
    private readonly Mock<ISalesDataReader> _readerMock = new();

    private GetDailySalesReportQueryHandler CreateHandler()
    {
        return new GetDailySalesReportQueryHandler(_readerMock.Object);
    }

    private void SetupAggregate(DateOnly date, decimal revenue, int count)
    {
        _readerMock.Setup(r => r.GetAggregateAsync(
                It.Is<DateTime>(d => d.Date == date.ToDateTime(TimeOnly.MinValue).Date),
                It.IsAny<DateTime>(), default))
            .ReturnsAsync(new SalesAggregate(revenue, count));
    }

    private void SetupPrevious(decimal revenue)
    {
        _readerMock.Setup(r => r.GetAggregateAsync(
                It.Is<DateTime>(d =>
                    d.Date != DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.MinValue).Date),
                It.IsAny<DateTime>(), default))
            .ReturnsAsync(new SalesAggregate(revenue, 0));
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithSalesData_ReturnsDtoWithRevenue()
    {
        var date = new DateOnly(2026, 3, 24);
        _readerMock.Setup(r => r.GetAggregateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), default))
            .ReturnsAsync(new SalesAggregate(500_000m, 10));
        _readerMock.Setup(r => r.GetTopProductsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 5, default))
            .ReturnsAsync([]);
        _readerMock.Setup(r => r.GetHourlySalesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), default))
            .ReturnsAsync([]);

        var result = await CreateHandler().Handle(new GetDailySalesReportQuery(date), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalRevenue.Should().Be(500_000m);
        result.Value.TransactionCount.Should().Be(10);
    }

    // ── Revenue change percent ────────────────────────────────────────────────

    [Fact]
    public async Task Handle_RevenueIncrease_PositiveChangePercent()
    {
        var date = new DateOnly(2026, 3, 24);
        var utcFrom = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var prevFrom = utcFrom.AddDays(-1);

        // current day
        _readerMock.Setup(r => r.GetAggregateAsync(
                It.Is<DateTime>(d => d >= utcFrom),
                It.IsAny<DateTime>(), default))
            .ReturnsAsync(new SalesAggregate(1_100_000m, 11));

        // previous day
        _readerMock.Setup(r => r.GetAggregateAsync(
                It.Is<DateTime>(d => d < utcFrom),
                It.IsAny<DateTime>(), default))
            .ReturnsAsync(new SalesAggregate(1_000_000m, 10));

        _readerMock.Setup(r => r.GetTopProductsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 5, default))
            .ReturnsAsync([]);
        _readerMock.Setup(r => r.GetHourlySalesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), default))
            .ReturnsAsync([]);

        var result = await CreateHandler().Handle(new GetDailySalesReportQuery(date), default);

        result.Value!.RevenueChangePercent.Should().Be(10m); // +10%
    }

    [Fact]
    public async Task Handle_PreviousDayZero_ChangePercentIsZero()
    {
        _readerMock.Setup(r => r.GetAggregateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), default))
            .ReturnsAsync(new SalesAggregate(500_000m, 5));
        // Override previous day to return 0
        _readerMock.SetupSequence(r => r.GetAggregateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), default))
            .ReturnsAsync(new SalesAggregate(500_000m, 5)) // current
            .ReturnsAsync(new SalesAggregate(0m, 0)); // previous

        _readerMock.Setup(r => r.GetTopProductsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 5, default))
            .ReturnsAsync([]);
        _readerMock.Setup(r => r.GetHourlySalesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), default))
            .ReturnsAsync([]);

        var result = await CreateHandler().Handle(
            new GetDailySalesReportQuery(new DateOnly(2026, 3, 24)), default);

        result.Value!.RevenueChangePercent.Should().Be(0);
    }

    // ── Top products mapping ──────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithTopProducts_MapsToDto()
    {
        _readerMock.Setup(r => r.GetAggregateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), default))
            .ReturnsAsync(new SalesAggregate(0m, 0));
        _readerMock.Setup(r => r.GetTopProductsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 5, default))
            .ReturnsAsync([new TopProduct("Coca Cola", "Lon 330ml", 50, 600_000m)]);
        _readerMock.Setup(r => r.GetHourlySalesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), default))
            .ReturnsAsync([]);

        var result = await CreateHandler().Handle(
            new GetDailySalesReportQuery(new DateOnly(2026, 3, 24)), default);

        result.Value!.TopSellingProducts.Should().ContainSingle(p =>
            p.ProductName == "Coca Cola" && p.QuantitySold == 50);
    }

    // ── Date range passed correctly ───────────────────────────────────────────

    [Fact]
    public async Task Handle_VerifiesCorrectDateRangePassedToReader()
    {
        var date = new DateOnly(2026, 6, 15);
        _readerMock.Setup(r => r.GetAggregateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), default))
            .ReturnsAsync(new SalesAggregate(0m, 0));
        _readerMock.Setup(r => r.GetTopProductsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 5, default))
            .ReturnsAsync([]);
        _readerMock.Setup(r => r.GetHourlySalesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), default))
            .ReturnsAsync([]);

        await CreateHandler().Handle(new GetDailySalesReportQuery(date), default);

        _readerMock.Verify(r => r.GetAggregateAsync(
            It.Is<DateTime>(d => d.Year == 2026 && d.Month == 6 && d.Day == 15),
            It.Is<DateTime>(d => d.Year == 2026 && d.Month == 6 && d.Day == 15),
            default), Times.AtLeastOnce);
    }
}