using Finance.Domain.Enums;
using Shared.Domain.Common;

namespace Finance.Domain.Entities;

public class Expense : BaseEntity
{
    private Expense() { }

    public ExpenseCategory Category { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = null!;
    public DateTime ExpenseDate { get; private set; }
    public string? ReceiptNumber { get; private set; }
    public ExpensePaymentMethod PaymentMethod { get; private set; }

    public static Expense Create(
        ExpenseCategory category,
        decimal amount,
        string description,
        DateTime expenseDate,
        ExpensePaymentMethod paymentMethod,
        string? receiptNumber = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Số tiền phải lớn hơn 0.", nameof(amount));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Mô tả không được để trống.", nameof(description));

        return new Expense
        {
            Category = category,
            Amount = amount,
            Description = description.Trim(),
            // Treat Unspecified as UTC (client sends a business date without timezone suffix).
            // Only convert to UTC if the value is explicitly marked as Local.
            ExpenseDate = expenseDate.Kind == DateTimeKind.Local
                ? expenseDate.ToUniversalTime()
                : DateTime.SpecifyKind(expenseDate, DateTimeKind.Utc),
            PaymentMethod = paymentMethod,
            ReceiptNumber = receiptNumber?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }
}
