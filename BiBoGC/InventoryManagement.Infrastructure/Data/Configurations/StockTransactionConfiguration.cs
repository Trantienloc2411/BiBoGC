using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryManagement.Infrastructure.Data.Configurations
{
    public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
    {
        public void Configure(EntityTypeBuilder<StockTransaction> builder)
        {
            builder.ToTable("StockTransactions");

            builder.HasKey(st => st.Id);

            builder.Property(st => st.TransactionType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();  

            builder.Property(st => st.Quantity)
                .IsRequired();

            builder.Property(st => st.UnitPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(st => st.TransactionDate)
                .IsRequired();

            builder.HasOne(st => st.Product)
                .WithMany()
                .HasForeignKey(st => st.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(st => st.ProductBatch)
                .WithMany()
                .HasForeignKey(st => st.ProductBatchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(st => st.Supplier)
                .WithMany()
                .HasForeignKey(st => st.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(st => st.ProductId);
            builder.HasIndex(st => st.TransactionDate);


            // Soft Delete filter
            builder.HasQueryFilter(st => !st.IsDeleted);

            // Ignore RowVersion - not needed for this application
            builder.Ignore(st => st.RowVersion);

            // Ignore domain events
            builder.Ignore(st => st.DomainEvents);


        }
    }
}
