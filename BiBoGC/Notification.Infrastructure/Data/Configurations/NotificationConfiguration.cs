using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Notification.Infrastructure.Data.Configurations;

public class NotificationConfiguration
    : IEntityTypeConfiguration<Domain.Entities.Notification>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Message)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ReferenceType)
            .HasMaxLength(50);

        // Ignore EF concurrency token — notifications don't need optimistic concurrency
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);

        // Indexes for the two query patterns used by the API
        builder.HasIndex(x => x.CreatedAt).IsDescending();
        builder.HasIndex(x => new { x.IsRead, x.Role });
    }
}
