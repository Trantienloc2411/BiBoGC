using Shared.Domain.Common;

namespace InventoryManagement.Domain.Events.ProductEvents;

public class ProductCreatedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public string Sku { get; }

    public ProductCreatedEvent(Guid productId, string productName, string sku)
    {
        ProductId = productId;
        ProductName = productName;
        Sku = sku;
    }
}