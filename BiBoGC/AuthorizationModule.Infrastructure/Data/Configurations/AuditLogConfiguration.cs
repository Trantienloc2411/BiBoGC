using AuthorizationModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthorizationModule.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Username)
            .HasMaxLength(100);

        builder.Property(a => a.HttpMethod)
            .HasMaxLength(10);

        builder.Property(a => a.Endpoint)
            .HasMaxLength(500);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(a => a.Description)
            .HasMaxLength(1000);

        // Queries: all actions in a time range, sorted newest-first
        builder.HasIndex(a => a.Timestamp)
            .IsDescending();

        // Queries: audit trail for a specific user
        builder.HasIndex(a => new { a.UserId, a.Timestamp });

        // Queries: filter by operation type (Login, Product.Create, etc.)
        builder.HasIndex(a => a.Action);
    }
}
