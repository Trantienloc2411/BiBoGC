namespace Sale.Domain.Exceptions;

public class SalesOrderNotFoundException : Exception
{
    public SalesOrderNotFoundException(Guid orderId)
        : base($"Không tìm thấy đơn hàng với ID '{orderId}'.")
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }
}