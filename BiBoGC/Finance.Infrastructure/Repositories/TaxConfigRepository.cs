using Finance.Application.Interfaces;
using Finance.Domain.Entities;
using Finance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Repositories;

public class TaxConfigRepository : ITaxConfigRepository
{
    private readonly FinanceDbContext _context;

    public TaxConfigRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public Task<TaxConfiguration?> GetActiveAsync(CancellationToken ct = default) =>
        GetByNameAsync("VAT", ct);

    public Task<TaxConfiguration?> GetByNameAsync(string name, CancellationToken ct = default) =>
        _context.TaxConfigurations
            .Where(t => t.Name == name)
            .OrderByDescending(t => t.UpdatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task<TaxConfiguration> AddAsync(TaxConfiguration config, CancellationToken ct = default)
    {
        await _context.TaxConfigurations.AddAsync(config, ct);
        return config;
    }
}