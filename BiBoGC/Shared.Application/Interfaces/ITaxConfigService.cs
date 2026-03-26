namespace Shared.Application.Interfaces;

/// <summary>
/// Cross-module service: returns active tax rates for use during order completion and reporting.
/// Implemented in Finance.Infrastructure, consumed by Sale.Application and Finance.Application.
/// Returns 0 when the tax is disabled or not configured.
/// </summary>
public interface ITaxConfigService
{
    Task<decimal> GetActiveVatRateAsync(CancellationToken ct = default);
    Task<decimal> GetActivePitRateAsync(CancellationToken ct = default);
}