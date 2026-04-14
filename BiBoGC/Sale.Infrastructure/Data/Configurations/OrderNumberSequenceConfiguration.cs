using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sale.Domain.Domain;

namespace Sale.Infrastructure.Data.Configurations;

public class OrderNumberSequenceConfiguration : IEntityTypeConfiguration<OrderNumberSequence>
{
    public void Configure(EntityTypeBuilder<OrderNumberSequence> builder)
    {
        builder.ToTable("OrderNumberSequences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Date)
            .IsRequired()
            .HasMaxLength(8);

        builder.Property(x => x.CurrentSequence)
            .IsRequired();

        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => x.Date).IsUnique();
    }
}
