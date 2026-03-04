using Shared.Domain.Common;

namespace Sale.Domain.Events;

public class SalesOrderCompletedEvent : DomainEvent
{
    public SalesOrderCompletedEvent(
        Guid orderId,
        string orderNumber,
        decimal totalAmount,
        IReadOnlyCollection<OrderItemSnapshot> items)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        TotalAmount = totalAmount;
        Items = items;
    }

    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public decimal TotalAmount { get; }
    public IReadOnlyCollection<OrderItemSnapshot> Items { get; }
}

public record OrderItemSnapshot(
    Guid ProductId,
    Guid ProductVariantId,
    Guid? ProductBatchId,
    int Quantity);