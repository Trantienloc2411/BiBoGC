using Finance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;

namespace Finance.Infrastructure.Services;

/// <summary>
/// Cross-module implementation of ITaxConfigService.
/// Reads the active VAT rate from FinanceDbContext.
/// Consumed by Sale.Application via DI.
/// </summary>
public class TaxConfigService : ITaxConfigService
{
    private readonly FinanceDbContext _context;

    public TaxConfigService(FinanceDbContext context)
    {
        _context = context;
    }

    public Task<decimal> GetActiveVatRateAsync(CancellationToken ct = default) =>
        GetRateByNameAsync("VAT", ct);

    public Task<decimal> GetActivePitRateAsync(CancellationToken ct = default) =>
        GetRateByNameAsync("PIT", ct);

    private async Task<decimal> GetRateByNameAsync(string name, CancellationToken ct)
    {
        var config = await _context.TaxConfigurations
            .Where(t => t.Name == name)
            .OrderByDescending(t => t.UpdatedAt)
            .FirstOrDefaultAsync(ct);

        return config is { IsEnabled: true } ? config.Rate : 0m;
    }
}