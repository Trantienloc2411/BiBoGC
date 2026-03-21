using Finance.Application.DTOs;
using Finance.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.GetMonthlySalesReport;

public class GetMonthlySalesReportQueryHandler
    : IRequestHandler<GetMonthlySalesReportQuery, Result<MonthlySalesReportDto>>
{
    private readonly ISalesDataReader _salesDataReader;

    public GetMonthlySalesReportQueryHandler(ISalesDataReader salesDataReader)
    {
        _salesDataReader = salesDataReader;
    }

    public async Task<Result<MonthlySalesReportDto>> Handle(
        GetMonthlySalesReportQuery request,
        CancellationToken cancellationToken)
    {
        var (year, month) = (request.Year, request.Month);

        var utcFrom = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var utcTo   = utcFrom.AddMonths(1).AddTicks(-1);

        // Previous month
        var prevFrom = utcFrom.AddMonths(-1);
        var prevTo   = utcFrom.AddTicks(-1);

        // Same month last year
        var lyFrom = new DateTime(year - 1, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lyTo   = lyFrom.AddMonths(1).AddTicks(-1);

        var current  = await _salesDataReader.GetAggregateAsync(utcFrom, utcTo, cancellationToken);
        var previous = await _salesDataReader.GetAggregateAsync(prevFrom, prevTo, cancellationToken);
        var lastYear = await _salesDataReader.GetAggregateAsync(lyFrom, lyTo, cancellationToken);

        var topProducts = await _salesDataReader.GetTopProductsAsync(utcFrom, utcTo, 10, cancellationToken);
        var dailyBreakdown = await _salesDataReader.GetDailyBreakdownAsync(utcFrom, utcTo, cancellationToken);

        var momChange = previous.TotalRevenue == 0
            ? 0
            : Math.Round((current.TotalRevenue - previous.TotalRevenue) / previous.TotalRevenue * 100, 2);

        var yoyChange = lastYear.TotalRevenue == 0
            ? 0
            : Math.Round((current.TotalRevenue - lastYear.TotalRevenue) / lastYear.TotalRevenue * 100, 2);

        var dto = new MonthlySalesReportDto
        {
            Year                      = year,
            Month                     = month,
            TotalRevenue              = current.TotalRevenue,
            TransactionCount          = current.TransactionCount,
            PreviousMonthRevenue      = previous.TotalRevenue,
            PreviousMonthChangePercent = momChange,
            SameMonthLastYearRevenue  = lastYear.TotalRevenue,
            YoYChangePercent          = yoyChange,
            TopSellingProducts = topProducts
                .Select(p => new TopProductDto
                {
                    ProductName  = p.ProductName,
                    VariantName  = p.VariantName,
                    QuantitySold = p.QuantitySold,
                    Revenue      = p.Revenue
                })
                .ToList(),
            DailyBreakdown = dailyBreakdown
                .Select(d => new DailyBreakdownDto
                {
                    Day              = d.Day,
                    Revenue          = d.Revenue,
                    TransactionCount = d.TransactionCount
                })
                .ToList()
        };

        return Result<MonthlySalesReportDto>.Success(dto);
    }
}
