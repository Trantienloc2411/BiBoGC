using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sale.Domain.Domain;

namespace Sale.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.OrderNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.StoreName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.StoreAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.StorePhone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.StoreTaxCode)
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

        builder.Property(x => x.GrandTotal)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.AmountPaid)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.ChangeAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.PaymentMethod)
            .IsRequired()
            .HasMaxLength(20);

        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Invoice)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.InvoiceNumber).IsUnique();
        builder.HasIndex(x => x.SalesOrderId).IsUnique();
        builder.HasIndex(x => x.InvoiceDate);
        builder.HasIndex(x => x.CreatedAt);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}