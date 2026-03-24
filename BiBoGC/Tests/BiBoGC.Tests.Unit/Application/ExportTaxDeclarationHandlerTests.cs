using Finance.Application.Interfaces;
using Finance.Application.Queries.ExportTaxDeclaration;
using Finance.Application.ReadModels;
using FluentAssertions;
using Moq;
using Shared.Application.Interfaces;

namespace BiBoGC.Tests.Unit.Application;

public class ExportTaxDeclarationHandlerTests
{
    private readonly Mock<ITaxDeclarationExportService> _exportMock = new();
    private readonly Mock<INotificationService> _notificationMock = new();
    private readonly Mock<ISalesDataReader> _readerMock = new();
    private readonly Mock<ITaxConfigService> _taxMock = new();

    private ExportTaxDeclarationQueryHandler CreateHandler()
    {
        return new ExportTaxDeclarationQueryHandler(_readerMock.Object, _exportMock.Object, _taxMock.Object,
            _notificationMock.Object);
    }

    private void SetupRevenue(decimal revenue)
    {
        _readerMock.Setup(r => r.GetAggregateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), default))
            .ReturnsAsync(new SalesAggregate(revenue, 0));
    }

    private void SetupRates(decimal vat, decimal pit)
    {
        _taxMock.Setup(t => t.GetActiveVatRateAsync(default)).ReturnsAsync(vat);
        _taxMock.Setup(t => t.GetActivePitRateAsync(default)).ReturnsAsync(pit);
    }

    private void SetupExport()
    {
        _exportMock.Setup(e => e.GenerateTaxDeclarationDocx(It.IsAny<TaxDeclarationData>()))
            .Returns([0x50, 0x4B]);
        // minimal docx magic bytes
    }

    // ── Month validation ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(0, 12)] // monthFrom = 0
    [InlineData(13, 13)] // monthFrom = 13
    [InlineData(-1, 12)] // monthFrom negative
    public async Task Handle_InvalidMonthFrom_ReturnsFailure(int monthFrom, int monthTo)
    {
        var result = await CreateHandler().Handle(
            new ExportTaxDeclarationQuery(2026, monthFrom, monthTo), default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("bắt đầu"));
    }

    [Theory]
    [InlineData(6, 5)] // monthTo < monthFrom
    [InlineData(1, 13)] // monthTo = 13
    [InlineData(3, 2)] // monthTo = 2, monthFrom = 3
    public async Task Handle_InvalidMonthTo_ReturnsFailure(int monthFrom, int monthTo)
    {
        var result = await CreateHandler().Handle(
            new ExportTaxDeclarationQuery(2026, monthFrom, monthTo), default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("kết thúc"));
    }

    // ── Tax rate fallback ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_BothRatesConfigured_UsesConfiguredRates()
    {
        SetupRevenue(1_000_000m);
        SetupRates(0.08m, 0.02m); // custom rates
        SetupExport();

        TaxDeclarationData? captured = null;
        _exportMock.Setup(e => e.GenerateTaxDeclarationDocx(It.IsAny<TaxDeclarationData>()))
            .Callback<TaxDeclarationData>(d => captured = d)
            .Returns([0x50, 0x4B]);

        await CreateHandler().Handle(new ExportTaxDeclarationQuery(2026, 1, 3), default);

        captured!.GtgtTaxAmount.Should().Be(80_000m); // 1,000,000 × 8%
        captured.TncnTaxAmount.Should().Be(20_000m); // 1,000,000 × 2%
    }

    [Fact]
    public async Task Handle_VatDisabled_FallsBackToStatutoryOnePercent()
    {
        SetupRevenue(1_000_000m);
        SetupRates(0m, 0.005m); // VAT returns 0 = disabled
        SetupExport();

        TaxDeclarationData? captured = null;
        _exportMock.Setup(e => e.GenerateTaxDeclarationDocx(It.IsAny<TaxDeclarationData>()))
            .Callback<TaxDeclarationData>(d => captured = d)
            .Returns([0x50, 0x4B]);

        await CreateHandler().Handle(new ExportTaxDeclarationQuery(2026, 1, 3), default);

        // Falls back to TT 40/2021 default 1%
        captured!.GtgtTaxAmount.Should().Be(10_000m); // 1,000,000 × 1%
    }

    [Fact]
    public async Task Handle_PitDisabled_FallsBackToStatutoryHalfPercent()
    {
        SetupRevenue(1_000_000m);
        SetupRates(0.01m, 0m); // PIT returns 0 = disabled
        SetupExport();

        TaxDeclarationData? captured = null;
        _exportMock.Setup(e => e.GenerateTaxDeclarationDocx(It.IsAny<TaxDeclarationData>()))
            .Callback<TaxDeclarationData>(d => captured = d)
            .Returns([0x50, 0x4B]);

        await CreateHandler().Handle(new ExportTaxDeclarationQuery(2026, 1, 3), default);

        // Falls back to TT 40/2021 default 0.5%
        captured!.TncnTaxAmount.Should().Be(5_000m); // 1,000,000 × 0.5%
    }

    [Fact]
    public async Task Handle_BothDisabled_FallsBackToBothStatutoryDefaults()
    {
        SetupRevenue(1_000_000m);
        SetupRates(0m, 0m);
        SetupExport();

        TaxDeclarationData? captured = null;
        _exportMock.Setup(e => e.GenerateTaxDeclarationDocx(It.IsAny<TaxDeclarationData>()))
            .Callback<TaxDeclarationData>(d => captured = d)
            .Returns([0x50, 0x4B]);

        await CreateHandler().Handle(new ExportTaxDeclarationQuery(2026, 1, 12), default);

        captured!.GtgtTaxAmount.Should().Be(10_000m);
        captured.TncnTaxAmount.Should().Be(5_000m);
    }

    // ── Zero revenue ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ZeroRevenue_TaxAmountsAreZero()
    {
        SetupRevenue(0m);
        SetupRates(0.01m, 0.005m);
        SetupExport();

        TaxDeclarationData? captured = null;
        _exportMock.Setup(e => e.GenerateTaxDeclarationDocx(It.IsAny<TaxDeclarationData>()))
            .Callback<TaxDeclarationData>(d => captured = d)
            .Returns([0x50, 0x4B]);

        await CreateHandler().Handle(new ExportTaxDeclarationQuery(2026, 1, 12), default);

        captured!.Revenue.Should().Be(0);
        captured.GtgtTaxAmount.Should().Be(0);
        captured.TncnTaxAmount.Should().Be(0);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidRequest_ReturnsDocxBytes()
    {
        SetupRevenue(5_000_000m);
        SetupRates(0.01m, 0.005m);
        SetupExport();

        var result = await CreateHandler().Handle(
            new ExportTaxDeclarationQuery(2026, 1, 12), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ValidRequest_PassesCorrectPeriodToDataReader()
    {
        SetupRevenue(0m);
        SetupRates(0.01m, 0.005m);
        SetupExport();

        await CreateHandler().Handle(new ExportTaxDeclarationQuery(2026, 3, 6), default);

        _readerMock.Verify(r => r.GetAggregateAsync(
            It.Is<DateTime>(d => d.Year == 2026 && d.Month == 3 && d.Day == 1),
            It.Is<DateTime>(d => d.Year == 2026 && d.Month == 6),
            default), Times.Once);
    }
}