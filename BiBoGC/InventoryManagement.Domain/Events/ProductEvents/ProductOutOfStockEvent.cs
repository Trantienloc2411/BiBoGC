using Shared.Domain.Common;

namespace InventoryManagement.Domain.Events.ProductEvents;

public class ProductOutOfStockEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }

    public ProductOutOfStockEvent(Guid productId, string productName)
    {
        ProductId = productId;
        ProductName = productName;
    }
}
