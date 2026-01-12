using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for ProductBatch entity
/// Defines table structure, constraints, and relationships
/// </summary>
public class ProductBatchConfiguration : IEntityTypeConfiguration<ProductBatch>
{
    public void Configure(EntityTypeBuilder<ProductBatch> builder)
    {
        builder.ToTable("ProductBatches");

        // Primary Key
        builder.HasKey(b => b.Id);

        // Foreign Key to Product
        builder.Property(b => b.ProductId)
            .IsRequired();

        // Properties
        builder.Property(b => b.BatchNumber)
            .IsRequired()
            .HasMaxLength(50);

        // Unique constraint: BatchNumber per Product
        builder.HasIndex(b => new { b.ProductId, b.BatchNumber })
            .IsUnique();

        builder.Property(b => b.Quantity)
            .IsRequired();

        builder.Property(b => b.ManufacturingDate)
            .IsRequired();

        builder.Property(b => b.ExpirationDate)
            .IsRequired();

        builder.Property(b => b.CostPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired()
            .HasDefaultValue(0m);

        // Audit fields
        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.UpdatedAt);

        builder.Property(b => b.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Ignore RowVersion for SQLite compatibility - use UpdatedAt instead for concurrency
        builder.Ignore(b => b.RowVersion);

        // Soft delete filter
        builder.HasQueryFilter(b => !b.IsDeleted);

        // Ignore domain events (not persisted)
        builder.Ignore(b => b.DomainEvents);
    }
}
