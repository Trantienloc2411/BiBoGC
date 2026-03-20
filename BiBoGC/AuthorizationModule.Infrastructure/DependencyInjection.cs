using AuthorizationModule.Application.Interfaces;
using AuthorizationModule.Infrastructure.Data;
using AuthorizationModule.Infrastructure.Security;
using AuthorizationModule.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuthorizationModule.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthorizationModule(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    public static async Task InitializeAuthorizationDatabaseAsync(this IServiceProvider serviceProvider, bool seedData = true)
    {
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AuthorizationDbContext>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<AuthorizationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        try
        {
            logger.LogInformation("Starting Authorization database initialization...");

            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            var migrations = pendingMigrations as string[] ?? pendingMigrations.ToArray();
            if (migrations.Length != 0)
            {
                logger.LogInformation("Applying {Count} pending migrations: {Migrations}",
                    migrations.Count(),
                    string.Join(", ", migrations));
                await context.Database.MigrateAsync();
                logger.LogInformation("Authorization database migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("No pending migrations found. Authorization database is up to date.");
            }

            // Seed authorization data if needed
            if (seedData) await AuthorizationDataSeeder.SeedAsync(context, logger, passwordHasher);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing Authorization database.");
            throw;
        }
    }
}