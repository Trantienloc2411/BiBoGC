using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariant");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id);

        builder.Property(p => p.ProductId)
            .HasColumnName("ProductId");

        builder.Property(p => p.VariantName)
            .HasColumnName("VariantName")
            .HasMaxLength(100)
            .IsRequired();
        ;

        builder.Property(p => p.SkuUnique)
            .HasConversion(
                sku => sku.Value,
                value => new Sku(value))
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.SkuUnique)
            .IsUnique();

        builder.Property(p => p.Barcode)
            .HasColumnName("Barcode")
            .HasMaxLength(200);

        builder.Property(p => p.Unit)
            .HasColumnName("Unit")
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(p => p.QuantityBaseUnit)
            .HasColumnName("QuantityBaseUnit")
            .HasConversion<int>()
            .IsRequired();
        ;

        builder.Property(p => p.SalePrice)
            .HasColumnName("SalePrice")
            .HasConversion(
                money => money.Value,
                money => new Money(money)
            )
            .HasColumnType("numeric(18,2)")
            .IsRequired();
        ;


        builder.Property(p => p.CostPrice)
            .HasColumnName("CostPrice")
            .HasConversion(
                money => money.Value,
                money => new Money(money)
            )
            .HasColumnType("numeric(18,2)");


        builder.Property(p => p.IsActive)
            .HasColumnName("IsActive")
            .IsRequired()
            .HasDefaultValue(true);
        builder.Property(p => p.DisplayOrder)
            .HasColumnName("DisplayOrder")
            .HasColumnType("int")
            .HasDefaultValue(0)
            .IsRequired();
        ;

        ;
        builder.HasQueryFilter(p => !p.IsDeleted);
        builder.Ignore(p => p.RowVersion);

        builder.Ignore(p => p.DomainEvents);


        builder.HasOne(p => p.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}