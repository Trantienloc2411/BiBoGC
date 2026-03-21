using Finance.Application.DTOs;
using Finance.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.GetMonthlyFinancialReport;

public class GetMonthlyFinancialReportQueryHandler
    : IRequestHandler<GetMonthlyFinancialReportQuery, Result<MonthlyFinancialReportDto>>
{
    private readonly ISalesDataReader  _salesDataReader;
    private readonly IStockDataReader  _stockDataReader;
    private readonly IExpenseRepository _expenseRepository;

    public GetMonthlyFinancialReportQueryHandler(
        ISalesDataReader salesDataReader,
        IStockDataReader stockDataReader,
        IExpenseRepository expenseRepository)
    {
        _salesDataReader   = salesDataReader;
        _stockDataReader   = stockDataReader;
        _expenseRepository = expenseRepository;
    }

    public async Task<Result<MonthlyFinancialReportDto>> Handle(
        GetMonthlyFinancialReportQuery request,
        CancellationToken cancellationToken)
    {
        var utcFrom = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var utcTo   = utcFrom.AddMonths(1).AddTicks(-1);

        var salesTask    = _salesDataReader.GetAggregateAsync(utcFrom, utcTo, cancellationToken);
        var cogsTask     = _stockDataReader.GetCogsAsync(utcFrom, utcTo, cancellationToken);
        var expenseTask  = _expenseRepository.GetTotalAmountAsync(utcFrom, utcTo, cancellationToken);

        await Task.WhenAll(salesTask, cogsTask, expenseTask);

        var revenue      = salesTask.Result.TotalRevenue;
        var cogs         = cogsTask.Result;
        var expenses     = expenseTask.Result;
        var grossProfit  = revenue - cogs;
        var netProfit    = grossProfit - expenses;
        var marginPct    = revenue == 0
            ? 0
            : Math.Round(netProfit / revenue * 100, 2);

        var dto = new MonthlyFinancialReportDto
        {
            Year                = request.Year,
            Month               = request.Month,
            TotalRevenue        = revenue,
            TotalCogs           = cogs,
            TotalExpenses       = expenses,
            GrossProfit         = grossProfit,
            NetProfit           = netProfit,
            ProfitMarginPercent = marginPct
        };

        return Result<MonthlyFinancialReportDto>.Success(dto);
    }
}
