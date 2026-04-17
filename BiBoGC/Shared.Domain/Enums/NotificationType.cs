namespace Shared.Domain.Enums;

public enum NotificationType
{
    Info = 0,
    Warning = 1,
    Error = 2,

    // Order events
    NewOrder = 10,
    OrderCancelled = 11,
    OrderCompleted = 12,

    // Stock events
    StockReceived = 20,
    LowStock = 21,
    OutOfStock = 22,
    BatchAdded = 23,

    // Product events
    PriceChanged = 30,
    ProductDiscontinued = 31,
}