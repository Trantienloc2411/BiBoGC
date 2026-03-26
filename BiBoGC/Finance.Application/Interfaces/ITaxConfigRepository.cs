using Finance.Domain.Entities;

namespace Finance.Application.Interfaces;

public interface ITaxConfigRepository
{
    Task<TaxConfiguration?> GetActiveAsync(CancellationToken ct = default);
    Task<TaxConfiguration?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<TaxConfiguration> AddAsync(TaxConfiguration config, CancellationToken ct = default);
}