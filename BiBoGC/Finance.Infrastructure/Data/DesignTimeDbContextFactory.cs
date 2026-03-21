using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Finance.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FinanceDbContext>
{
    public FinanceDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../BiBoGC"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("FinanceDb")
            ?? configuration.GetConnectionString("InventoryDb")
            ?? configuration.GetConnectionString("DesignTimeConnection")
            ?? throw new InvalidOperationException(
                "No connection string found. Provide 'FinanceDb', 'InventoryDb', or 'DesignTimeConnection'.");

        var optionsBuilder = new DbContextOptionsBuilder<FinanceDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            npgsqlOptions.MigrationsAssembly(typeof(FinanceDbContext).Assembly.FullName));

        return new FinanceDbContext(optionsBuilder.Options);
    }
}
