using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Notification.Infrastructure.Data;

/// <summary>
/// Used only by EF Core CLI tooling for migrations.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("NotificationDb")
            ?? configuration.GetConnectionString("InventoryDb")
            ?? "Host=localhost;Database=bibo_notification;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
        optionsBuilder.UseNpgsql(connectionString, o =>
            o.MigrationsAssembly(typeof(NotificationDbContext).Assembly.FullName));

        return new NotificationDbContext(optionsBuilder.Options);
    }
}
