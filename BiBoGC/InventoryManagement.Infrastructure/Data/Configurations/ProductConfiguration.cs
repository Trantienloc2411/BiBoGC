using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Product entity
/// Defines table structure, constraints, and relationships
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        // Primary Key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);
        builder.Property(p => p.SkuGeneral)
            .HasConversion(
                sku => sku.Value,
                value => new Sku(value)
                )
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.SkuGeneral)
            .IsUnique()
            .HasDatabaseName("IX_Products_SkuGeneral");
        builder.Property(p => p.CategoryId);


        builder.Property(p => p.TotalStock);
        builder.Property(p => p.AverageCostPrice)
            .HasConversion(
                money => money.Value,
                value => new Money(value))
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.BasePrice)
            .HasConversion(
                money => money.Value,
                value => new Money(value))
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.LowStockThreshold)
            .HasDefaultValue(0);
        
        // Enum: ProductStatus
        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.RequiresBatchTracking)
            .IsRequired()
            .HasDefaultValue(false);

        // Audit fields
        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt);

        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Ignore RowVersion - not needed for this application
        builder.Ignore(p => p.RowVersion);

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Soft delete filter
        builder.HasQueryFilter(p => !p.IsDeleted);

        // Relationship: Product has many ProductBatches
        builder.HasMany(p => p.Batches)
            .WithOne(b => b.Product)
            .HasForeignKey(b => b.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events (not persisted)
        builder.Ignore(p => p.DomainEvents);
        
 
    }
}
