using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryManagement.Infrastructure.Data.Configurations
{
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.ToTable("Suppliers");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.HasIndex(s => s.Name)
                   .IsUnique();

            builder.Property(s => s.ContactPerson)
                .HasMaxLength(200);
            builder.Property(s => s.PhoneNumber)
                .HasMaxLength(10);
            builder.Property(s => s.Address)
                .HasMaxLength(500);

            builder.Property(s => s.IsActive)
                   .IsRequired()
                   .HasDefaultValue(true);
            builder.HasQueryFilter(s => !s.IsDeleted);


            builder.Ignore(s => s.Transactions);
            builder.Ignore(s => s.DomainEvents);
            


        }
    }
}
