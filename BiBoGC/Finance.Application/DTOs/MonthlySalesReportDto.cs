namespace Finance.Application.DTOs;

public class MonthlySalesReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TransactionCount { get; set; }
    public decimal PreviousMonthRevenue { get; set; }
    public decimal PreviousMonthChangePercent { get; set; }
    public decimal SameMonthLastYearRevenue { get; set; }
    public decimal YoYChangePercent { get; set; }
    public List<DailyBreakdownDto> DailyBreakdown { get; set; } = [];
    public List<TopProductDto> TopSellingProducts { get; set; } = [];
}

public class DailyBreakdownDto
{
    public int Day { get; set; }
    public decimal Revenue { get; set; }
    public int TransactionCount { get; set; }
}
