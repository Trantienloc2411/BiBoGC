using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Data;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Repositories;
using Notification.Infrastructure.Services;
using Shared.Application.Interfaces;

namespace Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(NotificationDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            }));

        RegisterServices(services);
        return services;
    }

    public static IServiceCollection AddNotificationInfrastructureWithAspire(
        this IServiceCollection services)
    {
        RegisterServices(services);
        return services;
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationUnitOfWork, NotificationUnitOfWork>();
        services.AddScoped<INotificationService, NotificationService>();
    }

    public static async Task InitializeNotificationDatabaseAsync(
        this IServiceProvider serviceProvider)
    {
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger<NotificationDbContext>();

        try
        {
            logger.LogInformation("Starting Notification database initialization...");
            var pending = await context.Database.GetPendingMigrationsAsync();
            if (pending.Any())
            {
                logger.LogInformation("Applying {Count} pending Notification migration(s).", pending.Count());
                await context.Database.MigrateAsync();
                logger.LogInformation("Notification database migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("Notification database is up to date.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Notification database initialization failed.");
            throw;
        }
    }
}
