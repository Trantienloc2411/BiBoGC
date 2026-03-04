using Sale.Domain.Enum;

namespace Sale.Domain.Exceptions;

public class InvalidOrderStateException : Exception
{
    public InvalidOrderStateException(Guid orderId, OrderStatus currentStatus, string operation)
        : base($"Không thể thực hiện '{operation}' khi đơn hàng ở trạng thái '{currentStatus}'.")
    {
        OrderId = orderId;
        CurrentStatus = currentStatus;
        Operation = operation;
    }

    public Guid OrderId { get; }
    public OrderStatus CurrentStatus { get; }
    public string Operation { get; }
}