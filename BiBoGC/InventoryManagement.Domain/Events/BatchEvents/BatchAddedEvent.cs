using Shared.Domain.Common;

namespace InventoryManagement.Domain.Events.BatchEvents;

public class BatchAddedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public Guid BatchId { get; }
    public int Quantity { get; }

    public BatchAddedEvent(Guid productId, Guid batchId, int quantity)
    {
        ProductId = productId;
        BatchId = batchId;
        Quantity = quantity;
    }
}