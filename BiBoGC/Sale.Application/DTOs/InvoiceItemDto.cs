namespace Sale.Application.DTOs;

public class InvoiceItemDto
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = null!;
    public string VariantName { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public string Unit { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}