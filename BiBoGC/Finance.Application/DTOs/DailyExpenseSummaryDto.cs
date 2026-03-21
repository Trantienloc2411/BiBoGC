namespace Finance.Application.DTOs;

public class DailyExpenseSummaryDto
{
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
    public List<ExpenseCategoryBreakdownDto> BreakdownByCategory { get; set; } = [];
    public List<ExpenseDto> Expenses { get; set; } = [];
}

public class ExpenseCategoryBreakdownDto
{
    public string Category { get; set; } = null!;
    public decimal Total { get; set; }
    public int Count { get; set; }
}
