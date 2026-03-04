using Shared.Domain.Common;

namespace Sale.Domain.Domain;

/// <summary>
/// Chi tiết hóa đơn - Immutable
/// </summary>
public class InvoiceItem : BaseEntity
{
    private InvoiceItem()
    {
    }

    internal InvoiceItem(
        string productName,
        string variantName,
        string sku,
        string unit,
        int quantity,
        decimal unitPrice,
        decimal lineTotal)
    {
        ProductName = productName;
        VariantName = variantName;
        Sku = sku;
        Unit = unit;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = lineTotal;
    }

    internal void SetInvoiceId(Guid invoiceId)
    {
        InvoiceId = invoiceId;
    }

    #region Properties

    public Guid InvoiceId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public string VariantName { get; private set; } = null!;
    public string Sku { get; private set; } = null!;
    public string Unit { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }

    public Invoice Invoice { get; private set; } = null!;

    #endregion
}