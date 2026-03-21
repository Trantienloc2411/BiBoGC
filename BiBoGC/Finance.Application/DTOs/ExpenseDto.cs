namespace Finance.Application.DTOs;

public class ExpenseDto
{
    public Guid Id { get; set; }
    public string Category { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public DateTime ExpenseDate { get; set; }
    public string? ReceiptNumber { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
