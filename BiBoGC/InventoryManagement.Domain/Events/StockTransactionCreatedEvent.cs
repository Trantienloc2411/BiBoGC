using Shared.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryManagement.Domain.Events
{
    public class StockTransactionCreatedEvent : DomainEvent
    {
        public Guid StockTransactionId { get; }
        public Guid ProductId { get; }
        public int Quantity { get; }
        public string TransactionType { get; } = string.Empty;
    
        public StockTransactionCreatedEvent(
            Guid stockTransactionId,
            Guid productId,
            int quantity,
            string transactionType)
        {
            StockTransactionId = stockTransactionId;
            ProductId = productId;
            Quantity = quantity;
            TransactionType = transactionType;
        }
    }

}
