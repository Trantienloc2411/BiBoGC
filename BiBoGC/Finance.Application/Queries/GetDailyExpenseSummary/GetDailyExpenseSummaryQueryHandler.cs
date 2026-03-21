using Finance.Application.DTOs;
using Finance.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.GetDailyExpenseSummary;

public class GetDailyExpenseSummaryQueryHandler
    : IRequestHandler<GetDailyExpenseSummaryQuery, Result<DailyExpenseSummaryDto>>
{
    private readonly IExpenseRepository _expenseRepository;

    public GetDailyExpenseSummaryQueryHandler(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }

    public async Task<Result<DailyExpenseSummaryDto>> Handle(
        GetDailyExpenseSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var utcFrom = request.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var utcTo   = request.Date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var expenses   = await _expenseRepository.GetByDateRangeAsync(utcFrom, utcTo, cancellationToken);
        var total      = expenses.Sum(e => e.Amount);
        var breakdown  = await _expenseRepository.GetCategoryBreakdownAsync(utcFrom, utcTo, cancellationToken);

        var dto = new DailyExpenseSummaryDto
        {
            Date       = utcFrom.Date,
            TotalAmount = total,
            BreakdownByCategory = breakdown
                .Select(b => new ExpenseCategoryBreakdownDto
                {
                    Category = b.Category.ToString(),
                    Total    = b.Total,
                    Count    = b.Count
                })
                .ToList(),
            Expenses = expenses
                .Select(e => new ExpenseDto
                {
                    Id            = e.Id,
                    Category      = e.Category.ToString(),
                    Amount        = e.Amount,
                    Description   = e.Description,
                    ExpenseDate   = e.ExpenseDate,
                    ReceiptNumber = e.ReceiptNumber,
                    PaymentMethod = e.PaymentMethod.ToString(),
                    CreatedAt     = e.CreatedAt
                })
                .ToList()
        };

        return Result<DailyExpenseSummaryDto>.Success(dto);
    }
}
