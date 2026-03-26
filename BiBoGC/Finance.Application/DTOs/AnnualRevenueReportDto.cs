namespace Finance.Application.DTOs;

public class AnnualRevenueReportDto
{
    public int Year { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public List<MonthlyRevenueBreakdownDto> MonthlyBreakdowns { get; set; } = [];
}

public class MonthlyRevenueBreakdownDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = null!;
    public decimal Revenue { get; set; }
    public int TransactionCount { get; set; }

    /// <summary>Tăng trưởng so với tháng trước (null nếu là tháng 1 hoặc tháng trước = 0)</summary>
    public decimal? MoMGrowthPercent { get; set; }
}