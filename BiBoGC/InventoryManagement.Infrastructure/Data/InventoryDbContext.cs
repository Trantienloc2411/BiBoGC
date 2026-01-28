using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Data;

/// <summary>
/// Database context for Inventory Management module
/// Supports SQLite and PostgreSQL databases
/// </summary>
public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Products table - stores all product information
    /// </summary>
    public DbSet<Product> Products { get; set; } = null!;

    /// <summary>
    /// ProductBatches table - stores batch information for products with batch tracking
    /// </summary>
    public DbSet<ProductBatch> ProductBatches { get; set; } = null!;

    public DbSet<Supplier> Suppliers { get; set; } = null!;
    public DbSet<StockTransaction> StockTransactions { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<ProductVariant> ProductVariants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }
}