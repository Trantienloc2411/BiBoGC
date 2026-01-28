using Shared.Domain.Common;

namespace InventoryManagement.Domain.Events.ProductEvents;

public class ProductCreatedEvent : DomainEvent
{
    public ProductCreatedEvent(Guid productId, string productName)
    {
        ProductId = productId;
        ProductName = productName;
    }

    public Guid ProductId { get; }
    public string ProductName { get; }
}