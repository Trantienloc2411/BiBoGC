using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sale.Domain.Domain;

namespace Sale.Infrastructure.Data.Configurations;

public class SalesOrderItemConfiguration : IEntityTypeConfiguration<SalesOrderItem>
{
    public void Configure(EntityTypeBuilder<SalesOrderItem> builder)
    {
        builder.ToTable("SalesOrderItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.VariantName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Sku)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.Ignore(x => x.LineTotal);
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => x.SalesOrderId);
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.ProductVariantId);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}