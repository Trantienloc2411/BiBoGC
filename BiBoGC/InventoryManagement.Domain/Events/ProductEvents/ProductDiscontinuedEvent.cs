using Shared.Domain.Common;

namespace InventoryManagement.Domain.Events.ProductEvents;

public class ProductDiscontinuedEvent : DomainEvent
{
    public ProductDiscontinuedEvent(Guid productId, string productName)
    {
        ProductId = productId;
        ProductName = productName;
    }

    public Guid ProductId { get; }
    public string ProductName { get; }
}