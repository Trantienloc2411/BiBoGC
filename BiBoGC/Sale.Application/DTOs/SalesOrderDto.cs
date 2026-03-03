namespace Sale.Application.DTOs;

public class SalesOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public DateTime OrderDate { get; set; }

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public decimal AmountPaid { get; set; }
    public decimal ChangeAmount { get; set; }

    public string PaymentMethod { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }

    public Guid? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }

    public int ItemCount { get; set; }
    public IEnumerable<SalesOrderItemDto> Items { get; set; } = [];

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}