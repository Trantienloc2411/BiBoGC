using Shared.Domain.Common;

namespace Sale.Domain.Events;

public class SalesOrderCancelledEvent : DomainEvent
{
    public SalesOrderCancelledEvent(Guid orderId, string orderNumber, string? reason)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        Reason = reason;
    }

    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public string? Reason { get; }
}