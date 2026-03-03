using Shared.Domain.Common;

namespace Sale.Domain.Domain;

/// <summary>
/// Chi tiết đơn hàng bán
/// </summary>
public class SalesOrderItem : BaseEntity
{
    private SalesOrderItem()
    {
    }

    public SalesOrderItem(
        Guid salesOrderId,
        Guid productId,
        Guid productVariantId,
        Guid? productBatchId,
        string productName,
        string variantName,
        string sku,
        string unit,
        int quantity,
        decimal unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Số lượng phải lớn hơn 0.", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Đơn giá không được âm.", nameof(unitPrice));
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Tên sản phẩm không được để trống.", nameof(productName));

        SalesOrderId = salesOrderId;
        ProductId = productId;
        ProductVariantId = productVariantId;
        ProductBatchId = productBatchId;
        ProductName = productName.Trim();
        VariantName = variantName?.Trim() ?? string.Empty;
        Sku = sku?.Trim() ?? string.Empty;
        Unit = unit?.Trim() ?? "Cái";
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    #region Methods

    /// <summary>
    /// Cập nhật số lượng
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Số lượng phải lớn hơn 0.", nameof(newQuantity));

        Quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Properties

    public Guid SalesOrderId { get; private set; }
    public Guid ProductId { get; private set; }

    /// <summary>
    /// ProductVariant ID (bắt buộc - tính giá theo variant)
    /// </summary>
    public Guid ProductVariantId { get; private set; }

    /// <summary>
    /// ProductBatch ID (optional - cho sản phẩm theo dõi lô)
    /// </summary>
    public Guid? ProductBatchId { get; private set; }

    /// <summary>
    /// Tên sản phẩm (snapshot tại thời điểm bán)
    /// </summary>
    public string ProductName { get; private set; } = null!;

    /// <summary>
    /// Tên biến thể (snapshot)
    /// </summary>
    public string VariantName { get; private set; } = null!;

    /// <summary>
    /// SKU (snapshot)
    /// </summary>
    public string Sku { get; private set; } = null!;

    /// <summary>
    /// Đơn vị tính (Lon, Thùng, Kg, etc.)
    /// </summary>
    public string Unit { get; private set; } = null!;

    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Thành tiền = Quantity * UnitPrice
    /// </summary>
    public decimal LineTotal => Quantity * UnitPrice;

    // Navigation
    public SalesOrder SalesOrder { get; private set; } = null!;

    #endregion
}