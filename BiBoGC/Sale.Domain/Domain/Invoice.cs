using Sale.Domain.Enum;
using Sale.Domain.Events;
using Sale.Domain.ValueObjects;
using Shared.Domain.Common;

namespace Sale.Domain.Domain;

/// <summary>
/// Hóa đơn - Immutable sau khi tạo
/// </summary>
public class Invoice : BaseEntity
{
    private readonly List<InvoiceItem> _items = [];

    private Invoice()
    {
    }

    private Invoice(
        string invoiceNumber,
        Guid salesOrderId,
        string orderNumber,
        StoreInfo storeInfo,
        string? customerName,
        string? customerPhone,
        decimal subTotal,
        decimal discountAmount,
        decimal taxAmount,
        decimal grandTotal,
        decimal amountPaid,
        string paymentMethod,
        IEnumerable<InvoiceItem> items)
    {
        InvoiceNumber = invoiceNumber;
        InvoiceDate = DateTime.UtcNow;
        SalesOrderId = salesOrderId;
        OrderNumber = orderNumber;

        StoreName = storeInfo.Name;
        StoreAddress = storeInfo.Address;
        StorePhone = storeInfo.Phone;
        StoreTaxCode = storeInfo.TaxNumber;

        CustomerName = customerName;
        CustomerPhone = customerPhone;

        SubTotal = subTotal;
        DiscountAmount = discountAmount;
        TaxAmount = taxAmount;
        GrandTotal = grandTotal;
        AmountPaid = amountPaid;
        ChangeAmount = amountPaid - grandTotal;
        PaymentMethod = paymentMethod;

        foreach (var item in items)
        {
            _items.Add(item);
        }

        RaiseDomainEvent(new InvoiceGeneratedEvent(Id, InvoiceNumber, SalesOrderId, OrderNumber));
    }

    #region Factory Method

    /// <summary>
    /// Tạo Invoice từ SalesOrder đã hoàn thành
    /// </summary>
    public static Invoice CreateFromOrder(
        SalesOrder order,
        StoreInfo storeInfo,
        string invoiceNumber)
    {
        if (order.Status != OrderStatus.Completed)
            throw new InvalidOperationException("Chỉ có thể tạo hóa đơn cho đơn hàng đã hoàn thành.");

        var invoiceItems = order.Items.Select(item => new InvoiceItem(
            productName: item.ProductName,
            variantName: item.VariantName,
            sku: item.Sku,
            unit: item.Unit,
            quantity: item.Quantity,
            unitPrice: item.UnitPrice,
            lineTotal: item.LineTotal
        )).ToList();

        var invoice = new Invoice(
            invoiceNumber: invoiceNumber,
            salesOrderId: order.Id,
            orderNumber: order.OrderNumber,
            storeInfo: storeInfo,
            customerName: order.CustomerName,
            customerPhone: order.CustomerPhone,
            subTotal: order.SubTotal,
            discountAmount: order.DiscountAmount,
            taxAmount: order.TaxAmount,
            grandTotal: order.TotalAmount,
            amountPaid: order.AmountPaid,
            paymentMethod: order.PaymentMethod.ToString(),
            items: invoiceItems
        );

        foreach (var item in invoice._items)
        {
            item.SetInvoiceId(invoice.Id);
        }

        return invoice;
    }

    #endregion

    #region Properties

    public string InvoiceNumber { get; private set; } = null!;
    public DateTime InvoiceDate { get; private set; }
    public Guid SalesOrderId { get; private set; }
    public string OrderNumber { get; private set; } = null!;

    public string StoreName { get; private set; } = null!;
    public string StoreAddress { get; private set; } = null!;
    public string StorePhone { get; private set; } = null!;
    public string? StoreTaxCode { get; private set; }

    public string? CustomerName { get; private set; }
    public string? CustomerPhone { get; private set; }

    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal GrandTotal { get; private set; }
    public decimal AmountPaid { get; private set; }
    public decimal ChangeAmount { get; private set; }
    public string PaymentMethod { get; private set; } = null!;

    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    #endregion
}