using Shared.Domain.Common;

namespace Sale.Domain.Events;

public class InvoiceGeneratedEvent : DomainEvent
{
    public InvoiceGeneratedEvent(
        Guid invoiceId,
        string invoiceNumber,
        Guid orderId,
        string orderNumber)
    {
        InvoiceId = invoiceId;
        InvoiceNumber = invoiceNumber;
        OrderId = orderId;
        OrderNumber = orderNumber;
    }

    public Guid InvoiceId { get; }
    public string InvoiceNumber { get; }
    public Guid OrderId { get; }
    public string OrderNumber { get; }
}