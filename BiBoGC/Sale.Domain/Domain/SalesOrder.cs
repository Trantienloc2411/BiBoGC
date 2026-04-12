using Sale.Domain.Enum;
using Sale.Domain.Events;
using Sale.Domain.Exceptions;
using Shared.Domain.Common;

namespace Sale.Domain.Domain;

/// <summary>
/// Đơn hàng bán - Aggregate Root
/// </summary>
public class SalesOrder : BaseEntity
{
    private readonly List<SalesOrderItem> _items = [];

    private SalesOrder()
    {
    }

    public SalesOrder(
        string orderNumber,
        PaymentMethod paymentMethod,
        string? customerName = null,
        string? customerPhone = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Mã đơn hàng không được để trống.", nameof(orderNumber));

        OrderNumber = orderNumber.Trim().ToUpperInvariant();
        PaymentMethod = paymentMethod;
        CustomerName = customerName?.Trim();
        CustomerPhone = customerPhone?.Trim();
        Notes = notes?.Trim();
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Draft;

        RaiseDomainEvent(new SalesOrderCreatedEvent(Id, OrderNumber));
    }

    #region Properties

    public string OrderNumber { get; private set; } = null!;
    public string? CustomerName { get; private set; }
    public string? CustomerPhone { get; private set; }
    public DateTime OrderDate { get; private set; }

    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }

    public decimal AmountPaid { get; private set; }
    public decimal ChangeAmount => AmountPaid - TotalAmount;

    public PaymentMethod PaymentMethod { get; private set; }
    public OrderStatus Status { get; private set; }
    public string? Notes { get; private set; }

    public Guid? InvoiceId { get; private set; }

    public IReadOnlyCollection<SalesOrderItem> Items => _items.AsReadOnly();

    #endregion

    #region Item Management

    /// <summary>
    /// Thêm sản phẩm vào đơn hàng (bắt buộc phải có ProductVariant)
    /// </summary>
    public SalesOrderItem AddItem(
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
        EnsureDraftStatus("thêm sản phẩm");

        if (quantity <= 0)
            throw new ArgumentException("Số lượng phải lớn hơn 0.", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Đơn giá không được âm.", nameof(unitPrice));

        var existingItem = _items.FirstOrDefault(i =>
            i.ProductId == productId &&
            i.ProductVariantId == productVariantId &&
            i.ProductBatchId == productBatchId);

        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            RecalculateTotals();
            return existingItem;
        }

        var item = new SalesOrderItem(
            salesOrderId: Id,
            productId: productId,
            productVariantId: productVariantId,
            productBatchId: productBatchId,
            productName: productName,
            variantName: variantName,
            sku: sku,
            unit: unit,
            quantity: quantity,
            unitPrice: unitPrice);

        _items.Add(item);
        RecalculateTotals();
        return item;
    }

    /// <summary>
    /// Cập nhật số lượng của một item
    /// </summary>
    public void UpdateItemQuantity(Guid itemId, int newQuantity)
    {
        EnsureDraftStatus("cập nhật số lượng");

        var item = _items.FirstOrDefault(i => i.Id == itemId)
                   ?? throw new InvalidOperationException($"Không tìm thấy sản phẩm với ID '{itemId}' trong đơn hàng.");

        item.UpdateQuantity(newQuantity);
        RecalculateTotals();
    }

    /// <summary>
    /// Xóa item khỏi đơn hàng
    /// </summary>
    public void RemoveItem(Guid itemId)
    {
        EnsureDraftStatus("xóa sản phẩm");

        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            _items.Remove(item);
            RecalculateTotals();
        }
    }

    /// <summary>
    /// Trích xuất thuế từ tổng tiền (giá đã bao gồm thuế).
    /// TotalAmount KHÔNG thay đổi — khách hàng vẫn trả đúng số tiền ban đầu.
    /// TaxAmount chỉ mang tính chất kế toán/báo cáo.
    /// Công thức: taxAmount = total * rate / (1 + rate)
    /// </summary>
    public void ApplyTax(decimal taxRate)
    {
        EnsureDraftStatus("áp dụng thuế");

        if (taxRate < 0 || taxRate > 1)
            throw new ArgumentException("Thuế suất phải nằm trong khoảng 0 – 1.", nameof(taxRate));

        // Tax-inclusive pricing: extract the embedded tax — do NOT add on top
        var taxableAmount = SubTotal - DiscountAmount;
        TaxAmount = taxRate == 0
            ? 0m
            : Math.Round(taxableAmount * taxRate / (1 + taxRate), 2, MidpointRounding.AwayFromZero);

        UpdatedAt = DateTime.UtcNow;
        // TotalAmount is intentionally NOT recalculated — customer pays the same amount
    }

    /// <summary>
    /// Áp dụng giảm giá theo số tiền
    /// </summary>
    public void ApplyDiscount(decimal amount)
    {
        EnsureDraftStatus("áp dụng giảm giá");

        if (amount < 0)
            throw new ArgumentException("Số tiền giảm không được âm.", nameof(amount));
        if (amount > SubTotal)
            throw new ArgumentException("Số tiền giảm không được lớn hơn tổng tiền hàng.", nameof(amount));

        DiscountAmount = amount;
        RecalculateTotals();
    }

    /// <summary>
    /// Xóa giảm giá
    /// </summary>
    public void ClearDiscount()
    {
        EnsureDraftStatus("xóa giảm giá");
        DiscountAmount = 0;
        RecalculateTotals();
    }

    /// <summary>
    /// Cập nhật thông tin khách hàng
    /// </summary>
    public void UpdateCustomerInfo(string? name, string? phone)
    {
        EnsureDraftStatus("cập nhật thông tin khách hàng");

        CustomerName = name?.Trim();
        CustomerPhone = phone?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cập nhật ghi chú
    /// </summary>
    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cập nhật phương thức thanh toán
    /// </summary>
    public void UpdatePaymentMethod(PaymentMethod method)
    {
        EnsureDraftStatus("thay đổi phương thức thanh toán");
        PaymentMethod = method;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Order Lifecycle

    /// <summary>
    /// Hoàn thành đơn hàng
    /// </summary>
    public void Complete(decimal amountPaid)
    {
        EnsureDraftStatus("hoàn thành");

        if (_items.Count == 0)
            throw new InvalidOperationException("Không thể hoàn thành đơn hàng trống.");

        if (amountPaid < TotalAmount)
            throw new InvalidOperationException(
                $"Số tiền thanh toán ({amountPaid:N0}) không đủ. Cần thanh toán {TotalAmount:N0}.");

        AmountPaid = amountPaid;
        Status = OrderStatus.Completed;
        UpdatedAt = DateTime.UtcNow;

        var itemSnapshots = _items
            .Select(i => new OrderItemSnapshot(i.ProductId, i.ProductVariantId, i.ProductBatchId, i.Quantity))
            .ToList()
            .AsReadOnly();

        RaiseDomainEvent(new SalesOrderCompletedEvent(Id, OrderNumber, TotalAmount, itemSnapshots));
    }

    /// <summary>
    /// Hoàn tác đơn hàng về trạng thái Nháp khi trừ tồn kho thất bại sau khi hoàn thành.
    /// </summary>
    public void RevertToDraft()
    {
        if (Status != OrderStatus.Completed)
            throw new InvalidOperationException("Chỉ có thể hoàn tác đơn hàng đã hoàn thành.");

        Status = OrderStatus.Draft;
        AmountPaid = 0;
        TaxAmount = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Hủy đơn hàng
    /// </summary>
    public void Cancel(string? reason = null)
    {
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Đơn hàng đã bị hủy trước đó.");

        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Không thể hủy đơn hàng đã hoàn thành. Vui lòng tạo phiếu trả hàng.");

        Status = OrderStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Notes = string.IsNullOrEmpty(Notes)
                ? $"Lý do hủy: {reason}"
                : $"{Notes} | Lý do hủy: {reason}";
        }

        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new SalesOrderCancelledEvent(Id, OrderNumber, reason));
    }

    /// <summary>
    /// Liên kết với Invoice sau khi tạo
    /// </summary>
    public void SetInvoiceId(Guid invoiceId)
    {
        if (Status != OrderStatus.Completed)
            throw new InvalidOperationException("Chỉ có thể tạo hóa đơn cho đơn hàng đã hoàn thành.");

        if (InvoiceId.HasValue)
            throw new InvalidOperationException("Đơn hàng này đã có hóa đơn.");

        InvoiceId = invoiceId;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Private Methods

    private void EnsureDraftStatus(string operation)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOrderStateException(Id, Status, operation);
    }

    private void RecalculateTotals()
    {
        SubTotal = _items.Sum(i => i.LineTotal);
        TotalAmount = SubTotal - DiscountAmount;

        if (TotalAmount < 0)
            TotalAmount = 0;

        // Reset extracted tax whenever totals change — ApplyTax must be called again before Complete
        TaxAmount = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion
}