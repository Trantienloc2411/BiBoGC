using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Sale.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SaleDbContext>
{
    public SaleDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("SaleDb")
                               ?? "Host=localhost;Database=bibogc;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<SaleDbContext>();
        optionsBuilder.UseNpgsql(connectionString,
            npgsqlOptions => { npgsqlOptions.MigrationsAssembly(typeof(SaleDbContext).Assembly.FullName); });

        return new SaleDbContext(optionsBuilder.Options);
    }
}