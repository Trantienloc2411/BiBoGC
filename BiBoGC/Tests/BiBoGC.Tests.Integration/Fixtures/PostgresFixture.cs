using Finance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BiBoGC.Tests.Integration.Fixtures;

/// <summary>
/// Shared Postgres container spun up once per test class.
/// Each test class inherits from this and gets a fresh migrated DB.
/// </summary>
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("bibogc_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // Apply all EF migrations to the clean DB
        await using var ctx = CreateDbContext();
        await ctx.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }

    public FinanceDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new FinanceDbContext(options);
    }
}