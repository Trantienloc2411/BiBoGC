using Shared.Domain.Common;

namespace InventoryManagement.Domain.Events.ProductVariantEvents;

public class ProductVariantPriceChangedEvent : DomainEvent
{
    public ProductVariantPriceChangedEvent
        (Guid productVariantId, decimal oldPrice, decimal newPrice)
    {
        ProductVariantId = productVariantId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
    }

    public Guid ProductVariantId { get; }
    public decimal OldPrice { get; }
    public decimal NewPrice { get; }
}