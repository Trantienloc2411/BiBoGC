using Shared.Domain.Common;

namespace InventoryManagement.Domain.Events.ProductEvents;

public class ProductDiscontinuedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }

    public ProductDiscontinuedEvent(Guid productId, string productName)
    {
        ProductId = productId;
        ProductName = productName;
    }
    
}