using Shared.Domain.Common;

namespace InventoryManagement.Domain.Events.ProductEvents;

public class ProductLowStockEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public int CurrentStock { get; }
    public int Threshold { get; }

    public ProductLowStockEvent(Guid productId, string productName, int currentStock, int threshold)
    {
        ProductId = productId;
        ProductName = productName;
        CurrentStock = currentStock;
        Threshold = threshold;
    }
}
