namespace Sale.Application.DTOs;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public DateTime InvoiceDate { get; set; }
    public Guid SalesOrderId { get; set; }
    public string OrderNumber { get; set; } = null!;

    public string StoreName { get; set; } = null!;
    public string StoreAddress { get; set; } = null!;
    public string StorePhone { get; set; } = null!;
    public string? StoreTaxCode { get; set; }

    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal ChangeAmount { get; set; }
    public string PaymentMethod { get; set; } = null!;

    public IEnumerable<InvoiceItemDto> Items { get; set; } = [];

    public DateTime CreatedAt { get; set; }
}