using Shared.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sale.Domain.Domain
{
    public class SaleItem : BaseEntity
    {
        public Guid SaleId { get; private set; }
        public Guid ProductId { get; private set; }
        public Guid? ProductBatchId { get; private set; }
        public string ProductName { get; private set; } = null!;
        public string Sku { get; private set; } = null!;
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal LineTotal => Quantity * UnitPrice;

        //Navigation property
        public Sale Sale { get; private set; } = null!;

        private SaleItem() { } // For EF Core
        
        public SaleItem (
            Guid saleId, 
            Guid productId,
            Guid? productBatchId,
            string productName, 
            string sku,
            int quantity, 
            decimal unitPrice)
        {
            if(quantity <= 0)
                throw new ArgumentException("Số lượng phải lớn hơn 0.");
            if(unitPrice < 0)
                throw new ArgumentException("Đơn giá không được âm.");

            SaleId = saleId;
            ProductId = productId;
            ProductBatchId = productBatchId;
            ProductName = productName;
            Sku = sku;
            Quantity = quantity;
            UnitPrice = unitPrice;
            ProductBatchId = productBatchId;

        }

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Số lượng phải lớn hơn 0.");
            Quantity = newQuantity;
            UpdatedAt = DateTime.UtcNow;

        }

    }
}
