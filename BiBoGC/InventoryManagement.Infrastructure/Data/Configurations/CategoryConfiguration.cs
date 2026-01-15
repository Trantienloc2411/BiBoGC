using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryManagement.Infrastructure.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(200);
            builder.Property(c => c.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);
            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            // Ignore RowVersion - not needed for this application
            builder.Ignore(c => c.RowVersion);

            builder.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(c => !c.IsDeleted);

            // Ignore domain events

            builder.Ignore(b => b.DomainEvents);
            builder.Ignore(b => b.SubCategories);
            builder.Ignore(b => b.Items);








        }
    }
}
