using FluentAssertions;
using Finance.Domain.Entities;

namespace BiBoGC.Tests.Unit.Domain;

public class TaxConfigurationTests
{
    // ── Create ────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("VAT", 0.01, true)]
    [InlineData("PIT", 0.005, true)]
    [InlineData("VAT", 0.0, false)]
    [InlineData("VAT", 1.0, true)]
    public void Create_WithValidData_CreatesEntity(string name, decimal rate, bool isEnabled)
    {
        var config = TaxConfiguration.Create(name, rate, isEnabled);

        config.Name.Should().Be(name);
        config.Rate.Should().Be(rate);
        config.IsEnabled.Should().Be(isEnabled);
        config.EffectiveFrom.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        config.Id.Should().NotBe(Guid.Empty);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_ThrowsArgumentException(string name)
    {
        var act = () => TaxConfiguration.Create(name, 0.01m, true);

        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Theory]
    [InlineData(-0.0001)]
    [InlineData(1.0001)]
    [InlineData(-1.0)]
    [InlineData(2.0)]
    public void Create_WithRateOutOfRange_ThrowsArgumentException(decimal rate)
    {
        var act = () => TaxConfiguration.Create("VAT", rate, true);

        act.Should().Throw<ArgumentException>().WithParameterName("rate");
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public void Update_WithValidData_UpdatesRateAndIsEnabled()
    {
        var config = TaxConfiguration.Create("VAT", 0.01m, true);

        config.Update(0.08m, false);

        config.Rate.Should().Be(0.08m);
        config.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public void Update_RefreshesEffectiveFrom()
    {
        var config = TaxConfiguration.Create("VAT", 0.01m, true);
        var before = config.EffectiveFrom;

        // Small delay to ensure timestamp differs
        Thread.Sleep(10);
        config.Update(0.02m, true);

        config.EffectiveFrom.Should().BeAfter(before);
    }

    [Theory]
    [InlineData(-0.0001)]
    [InlineData(1.0001)]
    public void Update_WithRateOutOfRange_ThrowsArgumentException(decimal rate)
    {
        var config = TaxConfiguration.Create("VAT", 0.01m, true);

        var act = () => config.Update(rate, true);

        act.Should().Throw<ArgumentException>().WithParameterName("rate");
    }

    [Fact]
    public void Update_DisablingConfig_RateCanRemain()
    {
        var config = TaxConfiguration.Create("VAT", 0.01m, true);

        config.Update(0.01m, false);

        config.Rate.Should().Be(0.01m);
        config.IsEnabled.Should().BeFalse();
    }
}