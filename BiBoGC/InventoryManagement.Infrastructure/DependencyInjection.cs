using InventoryManagement.Application.Interfaces;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories;
using InventoryManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure layer services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add Infrastructure layer services to DI container
    /// Configures PostgreSQL database and repositories
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="connectionString">PostgreSQL connection string</param>
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services,
        string connectionString)
    {
        // Register DbContext with PostgreSQL
        services.AddDbContext<InventoryDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    5,
                    TimeSpan.FromSeconds(30),
                    null);
            });
        });

        // Register repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductBatchRepository, ProductBatchRepository>();
        services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
        services.AddScoped<IProductExportService, ProductExportService>();

        return services;
    }


    /// <summary>
    /// Initialize database - applies migrations and seeds data
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider, bool seedData = true)
    {
        // Use IServiceScopeFactory for proper scope handling with Aspire
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<InventoryDbContext>();

        try
        {
            logger.LogInformation("Starting database initialization...");

            // Apply pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending migrations: {Migrations}",
                    pendingMigrations.Count(),
                    string.Join(", ", pendingMigrations));
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("No pending migrations found. Database is up to date.");
            }

            // Seed data if enabled
            if (seedData) await DataSeeder.SeedAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    /// <summary>
    /// Register repositories only (use when DbContext is registered by Aspire)
    /// </summary>
    public static IServiceCollection AddInventoryInfrastructureWithAspire(this IServiceCollection services)
    {
        // Register all repositories - DbContext is registered by Aspire
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductBatchRepository, ProductBatchRepository>();
        services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
        services.AddScoped<IProductExportService, ProductExportService>();

        return services;
    }
}