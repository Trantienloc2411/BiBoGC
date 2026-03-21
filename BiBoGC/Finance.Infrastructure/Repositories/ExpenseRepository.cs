using Finance.Application.Interfaces;
using Finance.Domain.Entities;
using Finance.Domain.Enums;
using Finance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly FinanceDbContext _context;

    public ExpenseRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Expense> AddAsync(Expense expense, CancellationToken ct = default)
    {
        await _context.Expenses.AddAsync(expense, ct);
        return expense;
    }

    public Task<List<Expense>> GetByDateRangeAsync(
        DateTime utcFrom, DateTime utcTo, CancellationToken ct = default)
    {
        return _context.Expenses
            .Where(e => e.ExpenseDate >= utcFrom && e.ExpenseDate <= utcTo)
            .OrderBy(e => e.ExpenseDate)
            .ToListAsync(ct);
    }

    public Task<decimal> GetTotalAmountAsync(
        DateTime utcFrom, DateTime utcTo, CancellationToken ct = default)
    {
        return _context.Expenses
            .Where(e => e.ExpenseDate >= utcFrom && e.ExpenseDate <= utcTo)
            .SumAsync(e => e.Amount, ct);
    }

    public async Task<List<ExpenseCategoryTotal>> GetCategoryBreakdownAsync(
        DateTime utcFrom, DateTime utcTo, CancellationToken ct = default)
    {
        return await _context.Expenses
            .Where(e => e.ExpenseDate >= utcFrom && e.ExpenseDate <= utcTo)
            .GroupBy(e => e.Category)
            .Select(g => new ExpenseCategoryTotal(g.Key, g.Sum(e => e.Amount), g.Count()))
            .ToListAsync(ct);
    }
}
