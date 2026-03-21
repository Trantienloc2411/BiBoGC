using InventoryManagement.Domain.Enums;
using Shared.Domain.Common;

namespace InventoryManagement.Domain.Entities;

public class StockTransaction : BaseEntity
{
    private StockTransaction()
    {
    } // For EF Core

    public StockTransaction(
        Guid productId,
        Guid? productBatchId,
        Guid? supplierId,
        StockTransactionType transactionType,
        int quantity,
        decimal unitPrice,
        DateTime transactionDate,
        string? notes = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));
        ProductId = productId;
        ProductBatchId = productBatchId;
        SupplierId = supplierId;
        TransactionType = transactionType;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TransactionDate = transactionDate.Kind == DateTimeKind.Local
            ? transactionDate.ToUniversalTime()
            : DateTime.SpecifyKind(transactionDate, DateTimeKind.Utc);
        Notes = notes?.Trim();
    }

    public Guid ProductId { get; private set; }
    public Guid? ProductBatchId { get; private set; }
    public Guid? SupplierId { get; private set; }
    public StockTransactionType TransactionType { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice => Quantity * UnitPrice;
    public DateTime TransactionDate { get; private set; }
    public string? Notes { get; private set; }

    // Navigation properties
    public Product Product { get; private set; } = null!;
    public ProductBatch? ProductBatch { get; private set; } = null!;
    public Supplier? Supplier { get; private set; } = null!;

    public bool IsInboundTransaction()
    {
        return TransactionType is
            StockTransactionType.Purchase or
            StockTransactionType.AdjustmentIn or
            StockTransactionType.Return;
    }

    public bool IsOutboundTransaction()
    {
        return TransactionType is
            StockTransactionType.Sale or
            StockTransactionType.AdjustmentOut or
            StockTransactionType.Damage or
            StockTransactionType.Expiry or
            StockTransactionType.SupplierReturn;
    }
}