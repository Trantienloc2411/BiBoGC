using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AuthorizationModule.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AuthorizationDbContext>
{
    public AuthorizationDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "BiBoGC");
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile("appsettings.Development.json", true)
            .Build();

        var connectionString = configuration.GetConnectionString("DesignTimeConnection") ??
                               "Host=localhost;Database=InventoryDb;Username=postgres;Password=postgres";
        var optionsBuilder = new DbContextOptionsBuilder<AuthorizationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new AuthorizationDbContext(optionsBuilder.Options);
    }
}