namespace Finance.Application.DTOs;

public class DailySalesReportDto
{
    public DateTime Date { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TransactionCount { get; set; }
    public decimal PreviousDayRevenue { get; set; }
    public decimal RevenueChangePercent { get; set; }
    public List<TopProductDto> TopSellingProducts { get; set; } = [];
    public List<HourlySalesDto> SalesByHour { get; set; } = [];
}

public class TopProductDto
{
    public string ProductName { get; set; } = null!;
    public string VariantName { get; set; } = null!;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public class HourlySalesDto
{
    public int Hour { get; set; }
    public decimal Revenue { get; set; }
    public int TransactionCount { get; set; }
}
