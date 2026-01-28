namespace Sale.Application.DTOs;

public class SaleDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal ChangeAmount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }
    public int ItemCount { get; set; }
    public IEnumerable<SaleItemDto> Items { get; set; } = new List<SaleItemDto>();
}