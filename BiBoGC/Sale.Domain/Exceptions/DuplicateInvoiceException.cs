namespace Sale.Domain.Exceptions;

public class DuplicateInvoiceException : Exception
{
    public DuplicateInvoiceException(Guid orderId, string existingInvoiceNumber)
        : base($"Đơn hàng này đã có hóa đơn: {existingInvoiceNumber}")
    {
        OrderId = orderId;
        ExistingInvoiceNumber = existingInvoiceNumber;
    }

    public Guid OrderId { get; }
    public string ExistingInvoiceNumber { get; }
}