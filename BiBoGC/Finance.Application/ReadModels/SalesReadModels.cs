namespace Finance.Application.ReadModels;

public record SalesAggregate(decimal TotalRevenue, int TransactionCount);

public record TopProduct(string ProductName, string VariantName, int QuantitySold, decimal Revenue);

public record HourlySales(int Hour, decimal Revenue, int TransactionCount);

public record DailySales(int Day, decimal Revenue, int TransactionCount);

public record MonthlySales(int Month, decimal Revenue, int TransactionCount);