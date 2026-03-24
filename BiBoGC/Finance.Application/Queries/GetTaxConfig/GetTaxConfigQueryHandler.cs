using Finance.Application.DTOs;
using Finance.Application.Interfaces;
using Finance.Domain.Entities;
using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.GetTaxConfig;

public class GetTaxConfigQueryHandler : IRequestHandler<GetTaxConfigQuery, Result<TaxConfigsDto>>
{
    private readonly ITaxConfigRepository _taxConfigRepository;

    public GetTaxConfigQueryHandler(ITaxConfigRepository taxConfigRepository)
    {
        _taxConfigRepository = taxConfigRepository;
    }

    public async Task<Result<TaxConfigsDto>> Handle(GetTaxConfigQuery request, CancellationToken cancellationToken)
    {
        var vat = await _taxConfigRepository.GetByNameAsync("VAT", cancellationToken);
        var pit = await _taxConfigRepository.GetByNameAsync("PIT", cancellationToken);

        return Result<TaxConfigsDto>.Success(new TaxConfigsDto
        {
            Vat = MapToDto(vat, "VAT"),
            Pit = MapToDto(pit, "PIT")
        });
    }

    private static TaxConfigDto MapToDto(TaxConfiguration? config, string defaultName) =>
        config is null
            ? new TaxConfigDto
            {
                Name = defaultName,
                Rate = 0,
                IsEnabled = false,
                EffectiveFrom = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
            : new TaxConfigDto
            {
                Id = config.Id,
                Name = config.Name,
                Rate = config.Rate,
                IsEnabled = config.IsEnabled,
                EffectiveFrom = config.EffectiveFrom,
                UpdatedAt = config.UpdatedAt ?? config.CreatedAt
            };
}