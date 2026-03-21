using Finance.Domain.Entities;
using Finance.Domain.Enums;

namespace Finance.Application.Interfaces;

public record ExpenseCategoryTotal(ExpenseCategory Category, decimal Total, int Count);

public interface IExpenseRepository
{
    Task<Expense> AddAsync(Expense expense, CancellationToken ct = default);

    Task<List<Expense>> GetByDateRangeAsync(
        DateTime utcFrom, DateTime utcTo, CancellationToken ct = default);

    Task<decimal> GetTotalAmountAsync(
        DateTime utcFrom, DateTime utcTo, CancellationToken ct = default);

    Task<List<ExpenseCategoryTotal>> GetCategoryBreakdownAsync(
        DateTime utcFrom, DateTime utcTo, CancellationToken ct = default);
}
