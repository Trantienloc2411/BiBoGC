namespace InventoryManagement.Application.DTOs;

public class StockTransactionDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public Guid? ProductBatchId { get; set; }
    public string? BatchNumber { get; set; }
    public Guid? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string Sku { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public string? ReferenceNumber { get; set; }
    public string TransactionType { get; set; } = null!;
    public DateTime TransactionDate { get; set; }
    public string? Notes { get; set; }
}