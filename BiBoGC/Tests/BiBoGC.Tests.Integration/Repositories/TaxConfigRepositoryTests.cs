using BiBoGC.Tests.Integration.Fixtures;
using Finance.Domain.Entities;
using Finance.Infrastructure.Repositories;
using FluentAssertions;

namespace BiBoGC.Tests.Integration.Repositories;

[Collection("postgres")]
public class TaxConfigRepositoryTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;

    public TaxConfigRepositoryTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task CleanTableAsync()
    {
        await using var ctx = _fixture.CreateDbContext();
        ctx.TaxConfigurations.RemoveRange(ctx.TaxConfigurations);
        await ctx.SaveChangesAsync();
    }

    // ── GetByNameAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByNameAsync_Vat_ReturnsOnlyVatRow()
    {
        await CleanTableAsync();
        await using var ctx = _fixture.CreateDbContext();
        var vat = TaxConfiguration.Create("VAT", 0.01m, true);
        var pit = TaxConfiguration.Create("PIT", 0.005m, true);
        await ctx.TaxConfigurations.AddRangeAsync(vat, pit);
        await ctx.SaveChangesAsync();

        var repo = new TaxConfigRepository(ctx);
        var result = await repo.GetByNameAsync("VAT");

        result.Should().NotBeNull();
        result!.Name.Should().Be("VAT");
        result.Rate.Should().Be(0.01m);
    }

    [Fact]
    public async Task GetByNameAsync_Pit_ReturnsOnlyPitRow()
    {
        await CleanTableAsync();
        await using var ctx = _fixture.CreateDbContext();
        var vat = TaxConfiguration.Create("VAT", 0.01m, true);
        var pit = TaxConfiguration.Create("PIT", 0.005m, true);
        await ctx.TaxConfigurations.AddRangeAsync(vat, pit);
        await ctx.SaveChangesAsync();

        var repo = new TaxConfigRepository(ctx);
        var result = await repo.GetByNameAsync("PIT");

        result.Should().NotBeNull();
        result!.Name.Should().Be("PIT");
        result.Rate.Should().Be(0.005m);
    }

    [Fact]
    public async Task GetByNameAsync_MissingName_ReturnsNull()
    {
        await CleanTableAsync();
        await using var ctx = _fixture.CreateDbContext();

        var repo = new TaxConfigRepository(ctx);
        var result = await repo.GetByNameAsync("VAT");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_MultipleVatRows_ReturnsMostRecentlyUpdated()
    {
        await CleanTableAsync();
        await using var ctx = _fixture.CreateDbContext();

        var older = TaxConfiguration.Create("VAT", 0.01m, true);
        await ctx.TaxConfigurations.AddAsync(older);
        await ctx.SaveChangesAsync();

        // Small delay so UpdatedAt differs
        await Task.Delay(20);

        older.Update(0.08m, true);
        await ctx.SaveChangesAsync();

        var newer = TaxConfiguration.Create("VAT", 0.05m, false);
        await ctx.TaxConfigurations.AddAsync(newer);
        await ctx.SaveChangesAsync();

        var repo = new TaxConfigRepository(ctx);
        var result = await repo.GetByNameAsync("VAT");

        // Should return the one with the latest UpdatedAt — which is `newer` (just inserted)
        result.Should().NotBeNull();
        result!.Rate.Should().Be(0.05m);
    }

    // ── AddAsync + round-trip ─────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_PersistsAllFields()
    {
        await CleanTableAsync();
        await using var ctx = _fixture.CreateDbContext();
        var repo = new TaxConfigRepository(ctx);

        var config = TaxConfiguration.Create("PIT", 0.005m, true);
        await repo.AddAsync(config);
        await ctx.SaveChangesAsync();

        await using var readCtx = _fixture.CreateDbContext();
        var saved = await new TaxConfigRepository(readCtx).GetByNameAsync("PIT");

        saved.Should().NotBeNull();
        saved!.Name.Should().Be("PIT");
        saved.Rate.Should().Be(0.005m);
        saved.IsEnabled.Should().BeTrue();
        saved.Id.Should().NotBe(Guid.Empty);
    }

    // ── GetActiveAsync backward-compat ────────────────────────────────────────

    [Fact]
    public async Task GetActiveAsync_ReturnsVatRow()
    {
        await CleanTableAsync();
        await using var ctx = _fixture.CreateDbContext();
        var vat = TaxConfiguration.Create("VAT", 0.01m, true);
        var pit = TaxConfiguration.Create("PIT", 0.005m, true);
        await ctx.TaxConfigurations.AddRangeAsync(vat, pit);
        await ctx.SaveChangesAsync();

        var repo = new TaxConfigRepository(ctx);
        var result = await repo.GetActiveAsync();

        result.Should().NotBeNull();
        result!.Name.Should().Be("VAT");
    }
}