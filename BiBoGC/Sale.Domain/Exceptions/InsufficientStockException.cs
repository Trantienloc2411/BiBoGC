namespace Sale.Domain.Exceptions;

public class InsufficientStockException : Exception
{
    public InsufficientStockException(
        Guid productId,
        Guid productVariantId,
        string productName,
        int requestedQuantity,
        int availableQuantity)
        : base($"Sản phẩm '{productName}' không đủ tồn kho. Tồn: {availableQuantity}, Yêu cầu: {requestedQuantity}")
    {
        ProductId = productId;
        ProductVariantId = productVariantId;
        ProductName = productName;
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
    }

    public Guid ProductId { get; }
    public Guid ProductVariantId { get; }
    public string ProductName { get; }
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }
}