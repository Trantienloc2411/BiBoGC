using AuthorizationModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthorizationModule.Infrastructure.Data;

public class AuthorizationDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public AuthorizationDbContext(DbContextOptions<AuthorizationDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthorizationDbContext).Assembly);
    }
}