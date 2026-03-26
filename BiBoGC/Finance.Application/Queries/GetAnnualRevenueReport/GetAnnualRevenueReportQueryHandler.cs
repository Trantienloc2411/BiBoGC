using Finance.Application.DTOs;
using Finance.Application.Interfaces;
using MediatR;
using Shared.Application.Common;
using System.Globalization;

namespace Finance.Application.Queries.GetAnnualRevenueReport;

public class GetAnnualRevenueReportQueryHandler
    : IRequestHandler<GetAnnualRevenueReportQuery, Result<AnnualRevenueReportDto>>
{
    private readonly ISalesDataReader _salesDataReader;

    public GetAnnualRevenueReportQueryHandler(ISalesDataReader salesDataReader)
    {
        _salesDataReader = salesDataReader;
    }

    public async Task<Result<AnnualRevenueReportDto>> Handle(
        GetAnnualRevenueReportQuery request,
        CancellationToken cancellationToken)
    {
        var monthlyData = await _salesDataReader.GetMonthlyBreakdownAsync(request.Year, cancellationToken);

        // Build a full 12-month list, filling zeros for months with no data
        var dataByMonth = monthlyData.ToDictionary(m => m.Month);

        var breakdowns = new List<MonthlyRevenueBreakdownDto>();
        for (var month = 1; month <= 12; month++)
        {
            dataByMonth.TryGetValue(month, out var data);
            breakdowns.Add(new MonthlyRevenueBreakdownDto
            {
                Month = month,
                MonthName = CultureInfo.GetCultureInfo("vi-VN").DateTimeFormat.GetMonthName(month),
                Revenue = data?.Revenue ?? 0,
                TransactionCount = data?.TransactionCount ?? 0
            });
        }

        // Calculate MoM growth percent
        for (var i = 1; i < breakdowns.Count; i++)
        {
            var prev = breakdowns[i - 1].Revenue;
            var curr = breakdowns[i].Revenue;
            breakdowns[i].MoMGrowthPercent = prev == 0
                ? null
                : Math.Round((curr - prev) / prev * 100, 2);
        }

        return Result<AnnualRevenueReportDto>.Success(new AnnualRevenueReportDto
        {
            Year = request.Year,
            TotalRevenue = breakdowns.Sum(b => b.Revenue),
            TotalTransactions = breakdowns.Sum(b => b.TransactionCount),
            MonthlyBreakdowns = breakdowns
        });
    }
}