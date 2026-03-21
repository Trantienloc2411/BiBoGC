using Finance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finance.Infrastructure.Data.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Category)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ExpenseDate)
            .IsRequired();

        builder.Property(x => x.ReceiptNumber)
            .HasMaxLength(100);

        builder.Property(x => x.PaymentMethod)
            .HasConversion<int>()
            .IsRequired();

        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => x.ExpenseDate);
        builder.HasIndex(x => x.Category);
        builder.HasIndex(x => x.CreatedAt);
    }
}
