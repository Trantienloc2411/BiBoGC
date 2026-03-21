using Finance.Application.DTOs;
using Finance.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.GetDailySalesReport;

public class GetDailySalesReportQueryHandler
    : IRequestHandler<GetDailySalesReportQuery, Result<DailySalesReportDto>>
{
    private readonly ISalesDataReader _salesDataReader;

    public GetDailySalesReportQueryHandler(ISalesDataReader salesDataReader)
    {
        _salesDataReader = salesDataReader;
    }

    public async Task<Result<DailySalesReportDto>> Handle(
        GetDailySalesReportQuery request,
        CancellationToken cancellationToken)
    {
        var utcFrom   = request.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var utcTo     = request.Date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        var prevFrom  = utcFrom.AddDays(-1);
        var prevTo    = utcTo.AddDays(-1);

        var current  = await _salesDataReader.GetAggregateAsync(utcFrom, utcTo, cancellationToken);
        var previous = await _salesDataReader.GetAggregateAsync(prevFrom, prevTo, cancellationToken);

        var topProducts = await _salesDataReader.GetTopProductsAsync(utcFrom, utcTo, 5, cancellationToken);
        var hourly      = await _salesDataReader.GetHourlySalesAsync(utcFrom, utcTo, cancellationToken);

        var changePercent = previous.TotalRevenue == 0
            ? 0
            : Math.Round((current.TotalRevenue - previous.TotalRevenue) / previous.TotalRevenue * 100, 2);

        var dto = new DailySalesReportDto
        {
            Date                 = utcFrom.Date,
            TotalRevenue         = current.TotalRevenue,
            TransactionCount     = current.TransactionCount,
            PreviousDayRevenue   = previous.TotalRevenue,
            RevenueChangePercent = changePercent,
            TopSellingProducts = topProducts
                .Select(p => new TopProductDto
                {
                    ProductName  = p.ProductName,
                    VariantName  = p.VariantName,
                    QuantitySold = p.QuantitySold,
                    Revenue      = p.Revenue
                })
                .ToList(),
            SalesByHour = hourly
                .Select(h => new HourlySalesDto
                {
                    Hour             = h.Hour,
                    Revenue          = h.Revenue,
                    TransactionCount = h.TransactionCount
                })
                .ToList()
        };

        return Result<DailySalesReportDto>.Success(dto);
    }
}
