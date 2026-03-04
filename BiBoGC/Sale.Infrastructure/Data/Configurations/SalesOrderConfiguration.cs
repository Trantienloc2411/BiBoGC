using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sale.Domain.Domain;

namespace Sale.Infrastructure.Data.Configurations;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.CustomerName)
            .HasMaxLength(200);

        builder.Property(x => x.CustomerPhone)
            .HasMaxLength(20);

        builder.Property(x => x.SubTotal)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.DiscountAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.TaxAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.TotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.AmountPaid)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.PaymentMethod)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.Property(x => x.Notes)
            .HasColumnType("text");

        builder.Ignore(x => x.ChangeAmount);
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.SalesOrder)
            .HasForeignKey(x => x.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.OrderNumber).IsUnique();
        builder.HasIndex(x => x.OrderDate);
        builder.HasIndex(x => x.Status).HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.CustomerPhone).HasFilter("\"CustomerPhone\" IS NOT NULL");
        builder.HasIndex(x => x.InvoiceId).IsUnique().HasFilter("\"InvoiceId\" IS NOT NULL");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}