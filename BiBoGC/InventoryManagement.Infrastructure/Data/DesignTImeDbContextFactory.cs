using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace InventoryManagement.Infrastructure.Data;

public class DesignTImeDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        // Tìm đường dẫn đến BiBoGC project
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "BiBoGC");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile("appsettings.Development.json", true)
            .Build();

        var connectionString = configuration.GetConnectionString("DesignTimeConnection")
                               ?? "Host=localhost;Database=InventoryDb;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new InventoryDbContext(optionsBuilder.Options);
    }
}