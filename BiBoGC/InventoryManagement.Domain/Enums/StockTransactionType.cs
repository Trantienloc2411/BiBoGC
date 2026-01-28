namespace InventoryManagement.Domain.Enums;

public enum StockTransactionType
{
    /// <summary>
    /// Import product from supplier to warehouse
    /// </summary>
    Purchase = 1,

    /// <summary>
    /// Export product from warehouse to customer
    /// </summary>
    Sale = 2,

    /// <summary>
    /// Adjustment to stock levels (e.g., inventory correction) increase
    /// </summary>
    AdjustmentIn = 3,

    /// <summary>
    /// Adjustment to stock levels (e.g., inventory correction) decrease
    /// </summary>
    AdjustmentOut = 4,

    /// <summary>
    /// Product damage or loss
    /// </summary>
    Damage = 5,

    /// <summary>
    /// Expired products removal
    /// </summary>
    Expiry = 6,

    /// <summary>
    /// Customer return
    /// </summary>
    Return = 7,

    /// <summary>
    /// Return to supplier
    /// </summary>
    SupplierReturn = 8
}