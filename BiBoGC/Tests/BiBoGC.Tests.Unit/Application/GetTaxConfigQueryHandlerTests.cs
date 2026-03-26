using Finance.Application.Interfaces;
using Finance.Application.Queries.GetTaxConfig;
using Finance.Domain.Entities;
using FluentAssertions;
using Moq;
using Shared.Application.Interfaces;

namespace BiBoGC.Tests.Unit.Application;

public class GetTaxConfigQueryHandlerTests
{
    private readonly Mock<INotificationService> _notificationMock = new();
    private readonly Mock<ITaxConfigRepository> _repoMock = new();

    private GetTaxConfigQueryHandler CreateHandler()
    {
        return new GetTaxConfigQueryHandler(_repoMock.Object, _notificationMock.Object);
    }

    [Fact]
    public async Task Handle_NeitherConfigExists_ReturnsBothAsDisabledDefaults()
    {
        _repoMock.Setup(r => r.GetByNameAsync("VAT", default))
            .ReturnsAsync((TaxConfiguration?)null);
        _repoMock.Setup(r => r.GetByNameAsync("PIT", default))
            .ReturnsAsync((TaxConfiguration?)null);

        var result = await CreateHandler().Handle(new GetTaxConfigQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Vat.Name.Should().Be("VAT");
        result.Value.Vat.IsEnabled.Should().BeFalse();
        result.Value.Vat.Rate.Should().Be(0);
        result.Value.Pit.Name.Should().Be("PIT");
        result.Value.Pit.IsEnabled.Should().BeFalse();
        result.Value.Pit.Rate.Should().Be(0);
    }

    [Fact]
    public async Task Handle_VatExistsPitMissing_ReturnsMappedVatAndDefaultPit()
    {
        var vatConfig = TaxConfiguration.Create("VAT", 0.01m, true);
        _repoMock.Setup(r => r.GetByNameAsync("VAT", default)).ReturnsAsync(vatConfig);
        _repoMock.Setup(r => r.GetByNameAsync("PIT", default))
            .ReturnsAsync((TaxConfiguration?)null);

        var result = await CreateHandler().Handle(new GetTaxConfigQuery(), default);

        result.Value!.Vat.Rate.Should().Be(0.01m);
        result.Value.Vat.IsEnabled.Should().BeTrue();
        result.Value.Vat.RatePercent.Should().Be(1m);
        result.Value.Pit.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_BothConfigsExist_ReturnsBothMapped()
    {
        var vat = TaxConfiguration.Create("VAT", 0.01m, true);
        var pit = TaxConfiguration.Create("PIT", 0.005m, true);
        _repoMock.Setup(r => r.GetByNameAsync("VAT", default)).ReturnsAsync(vat);
        _repoMock.Setup(r => r.GetByNameAsync("PIT", default)).ReturnsAsync(pit);

        var result = await CreateHandler().Handle(new GetTaxConfigQuery(), default);

        result.Value!.Vat.Rate.Should().Be(0.01m);
        result.Value.Pit.Rate.Should().Be(0.005m);
        result.Value.Vat.RatePercent.Should().Be(1m);
        result.Value.Pit.RatePercent.Should().Be(0.5m);
    }
}