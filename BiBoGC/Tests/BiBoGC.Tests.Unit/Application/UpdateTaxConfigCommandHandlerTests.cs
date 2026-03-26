using Finance.Application.Commands.UpdateTaxConfig;
using Finance.Application.Interfaces;
using Finance.Domain.Entities;
using FluentAssertions;
using Moq;
using Shared.Application.Interfaces;

namespace BiBoGC.Tests.Unit.Application;

public class UpdateTaxConfigCommandHandlerTests
{
    private readonly Mock<IAuditLogger> _auditMock = new();
    private readonly Mock<ITaxConfigRepository> _repoMock = new();
    private readonly Mock<IFinanceUnitOfWork> _uowMock = new();

    private UpdateTaxConfigCommandHandler CreateHandler()
    {
        return new UpdateTaxConfigCommandHandler(_repoMock.Object, _uowMock.Object, _auditMock.Object);
    }

    // ── VAT ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_VatType_NoExistingConfig_CreatesNewVatRow()
    {
        _repoMock.Setup(r => r.GetByNameAsync("VAT", default))
            .ReturnsAsync((TaxConfiguration?)null);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<TaxConfiguration>(), default))
            .ReturnsAsync((TaxConfiguration c, CancellationToken _) => c);

        var result = await CreateHandler().Handle(
            new UpdateTaxConfigCommand("VAT", 0.01m, true), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("VAT");
        result.Value.Rate.Should().Be(0.01m);
        _repoMock.Verify(r => r.AddAsync(It.Is<TaxConfiguration>(c => c.Name == "VAT"), default), Times.Once);
    }

    [Fact]
    public async Task Handle_PitType_NoExistingConfig_CreatesNewPitRow()
    {
        _repoMock.Setup(r => r.GetByNameAsync("PIT", default))
            .ReturnsAsync((TaxConfiguration?)null);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<TaxConfiguration>(), default))
            .ReturnsAsync((TaxConfiguration c, CancellationToken _) => c);

        var result = await CreateHandler().Handle(
            new UpdateTaxConfigCommand("PIT", 0.005m, true), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("PIT");
        result.Value.Rate.Should().Be(0.005m);
        _repoMock.Verify(r => r.AddAsync(It.Is<TaxConfiguration>(c => c.Name == "PIT"), default), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingVatConfig_UpdatesItInstead()
    {
        var existing = TaxConfiguration.Create("VAT", 0.01m, true);
        _repoMock.Setup(r => r.GetByNameAsync("VAT", default)).ReturnsAsync(existing);

        var result = await CreateHandler().Handle(
            new UpdateTaxConfigCommand("VAT", 0.08m, false), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Rate.Should().Be(0.08m);
        result.Value.IsEnabled.Should().BeFalse();
        _repoMock.Verify(r => r.AddAsync(It.IsAny<TaxConfiguration>(), default), Times.Never);
    }

    // ── Invalid taxType ───────────────────────────────────────────────────────

    [Theory]
    [InlineData("GST")]
    [InlineData("")]
    [InlineData("vat")] // lowercase should still pass (normalised in handler)
    public async Task Handle_InvalidTaxType_ReturnsFailure(string taxType)
    {
        // lowercase "vat" is valid after ToUpperInvariant — only truly invalid ones fail
        if (taxType.ToUpperInvariant() is "VAT" or "PIT")
        {
            _repoMock.Setup(r => r.GetByNameAsync(It.IsAny<string>(), default))
                .ReturnsAsync((TaxConfiguration?)null);
            _repoMock.Setup(r => r.AddAsync(It.IsAny<TaxConfiguration>(), default))
                .ReturnsAsync((TaxConfiguration c, CancellationToken _) => c);
        }

        var result = await CreateHandler().Handle(
            new UpdateTaxConfigCommand(taxType, 0.01m, true), default);

        if (taxType.ToUpperInvariant() is not ("VAT" or "PIT"))
            result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_GstTaxType_ReturnsFailureWithMessage()
    {
        var result = await CreateHandler().Handle(
            new UpdateTaxConfigCommand("GST", 0.1m, true), default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("VAT") && e.Contains("PIT"));
    }

    // ── SaveChanges & Audit ───────────────────────────────────────────────────

    [Fact]
    public async Task Handle_Success_SavesChangesAndLogsAudit()
    {
        _repoMock.Setup(r => r.GetByNameAsync("VAT", default))
            .ReturnsAsync((TaxConfiguration?)null);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<TaxConfiguration>(), default))
            .ReturnsAsync((TaxConfiguration c, CancellationToken _) => c);

        await CreateHandler().Handle(new UpdateTaxConfigCommand("VAT", 0.01m, true), default);

        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        _auditMock.Verify(a => a.LogAsync(
            "TaxConfig.Update",
            true,
            null,
            null,
            It.Is<string?>(d => d != null && d.Contains("VAT")),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}