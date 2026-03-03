using Shared.Domain.Common;

namespace Sale.Domain.Events;

public class SalesOrderCreatedEvent : DomainEvent
{
    public SalesOrderCreatedEvent(Guid orderId, string orderNumber)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
    }

    public Guid OrderId { get; }
    public string OrderNumber { get; }
}