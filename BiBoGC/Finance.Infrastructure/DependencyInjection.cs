using Finance.Application.Interfaces;
using Finance.Infrastructure.Data;
using Finance.Infrastructure.Persistence;
using Finance.Infrastructure.Repositories;
using Finance.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Finance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<FinanceDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(FinanceDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            }));

        RegisterServices(services);
        return services;
    }

    public static IServiceCollection AddFinanceInfrastructureWithAspire(this IServiceCollection services)
    {
        RegisterServices(services);
        return services;
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IFinanceUnitOfWork, FinanceUnitOfWork>();
        services.AddScoped<ISalesDataReader, SalesDataReader>();
        services.AddScoped<IStockDataReader, StockDataReader>();
    }

    public static async Task InitializeFinanceDatabaseAsync(this IServiceProvider serviceProvider)
    {
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<FinanceDbContext>();

        try
        {
            logger.LogInformation("Starting Finance database initialization...");

            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending Finance migrations: {Migrations}",
                    pendingMigrations.Count(),
                    string.Join(", ", pendingMigrations));
                await context.Database.MigrateAsync();
                logger.LogInformation("Finance database migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("No pending Finance migrations found. Database is up to date.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the Finance database.");
            throw;
        }
    }
}
