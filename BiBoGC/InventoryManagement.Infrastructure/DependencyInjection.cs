using InventoryManagement.Application.Interfaces;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagement.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure layer services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add Infrastructure layer services to DI container
    /// Configures SQLite database and repositories
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="connectionString">SQLite connection string</param>
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Register DbContext with SQLite
        services.AddDbContext<InventoryDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        // Register repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
        services.AddScoped<ISupplerRepository, SupplierRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        return services;
    }

    /// <summary>
    /// Initialize database - creates database and applies migrations
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
    }
}
