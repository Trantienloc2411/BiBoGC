using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sale.Domain.Domain;

namespace Sale.Infrastructure.Data.Configurations;

public class InvoiceNumberSequenceConfiguration : IEntityTypeConfiguration<InvoiceNumberSequence>
{
    public void Configure(EntityTypeBuilder<InvoiceNumberSequence> builder)
    {
        builder.ToTable("InvoiceNumberSequences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.YearMonth)
            .IsRequired()
            .HasMaxLength(6);

        builder.Property(x => x.CurrentSequence)
            .IsRequired();

        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => x.YearMonth).IsUnique();
    }
}