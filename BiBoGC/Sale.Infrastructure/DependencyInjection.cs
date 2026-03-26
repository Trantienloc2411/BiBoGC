using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuestPDF.Infrastructure;
using Sale.Application.Interfaces;
using Sale.Infrastructure.Data;
using Sale.Infrastructure.Repositories;
using Sale.Infrastructure.Services;

namespace Sale.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSaleInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        services.AddDbContext<SaleDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(SaleDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });
        });

        services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();

        services.AddScoped<IOrderNumberGenerator, OrderNumberGenerator>();
        services.AddScoped<IInvoiceNumberGenerator, InvoiceNumberGenerator>();
        services.AddScoped<IStoreInfoService, StoreInfoService>();
        services.AddScoped<ISaleUnitOfWork, SaleUnitOfWork>();
        services.AddScoped<IPdfExportService, PdfExportService>();

        return services;
    }

    public static IServiceCollection AddSaleInfrastructureWithAspire(this IServiceCollection services)
    {
        services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();

        services.AddScoped<IOrderNumberGenerator, OrderNumberGenerator>();
        services.AddScoped<IInvoiceNumberGenerator, InvoiceNumberGenerator>();
        services.AddScoped<IStoreInfoService, StoreInfoService>();
        services.AddScoped<ISaleUnitOfWork, SaleUnitOfWork>();
        services.AddScoped<IPdfExportService, PdfExportService>();

        return services;
    }

    /// <summary>
    /// Initialize Sale database - applies migrations
    /// </summary>
    public static async Task InitializeSaleDatabaseAsync(this IServiceProvider serviceProvider)
    {
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<SaleDbContext>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<SaleDbContext>();

        try
        {
            logger.LogInformation("Starting Sale database initialization...");

            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending Sale migrations: {Migrations}",
                    pendingMigrations.Count(),
                    string.Join(", ", pendingMigrations));
                await context.Database.MigrateAsync();
                logger.LogInformation("Sale database migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("No pending Sale migrations found. Database is up to date.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the Sale database.");
            throw;
        }
    }
}