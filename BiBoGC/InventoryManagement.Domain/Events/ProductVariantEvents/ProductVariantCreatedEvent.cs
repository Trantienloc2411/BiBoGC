using Shared.Domain.Common;

namespace InventoryManagement.Domain.Events.ProductVariantEvents;

public class ProductVariantCreatedEvent : DomainEvent
{
    public ProductVariantCreatedEvent
    (
        Guid productId,
        Guid productVariantId,
        string variantName
    )
    {
        ProductId = productId;
        ProductVariantId = productVariantId;
        VariantName = variantName;
    }

    public Guid ProductId { get; }
    public Guid ProductVariantId { get; }

    public string VariantName { get; }
}