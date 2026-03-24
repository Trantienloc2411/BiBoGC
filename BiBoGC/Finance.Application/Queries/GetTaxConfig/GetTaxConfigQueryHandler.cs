using Finance.Application.DTOs;
using Finance.Application.Interfaces;
using Finance.Domain.Entities;
using MediatR;
using Shared.Application.Common;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Finance.Application.Queries.GetTaxConfig;

public class GetTaxConfigQueryHandler : IRequestHandler<GetTaxConfigQuery, Result<TaxConfigsDto>>
{
    private readonly INotificationService _notificationService;
    private readonly ITaxConfigRepository _taxConfigRepository;

    public GetTaxConfigQueryHandler(
        ITaxConfigRepository taxConfigRepository,
        INotificationService notificationService)
    {
        _taxConfigRepository = taxConfigRepository;
        _notificationService = notificationService;
    }

    public async Task<Result<TaxConfigsDto>> Handle(
        GetTaxConfigQuery request,
        CancellationToken cancellationToken)
    {
        var vat = await _taxConfigRepository.GetByNameAsync("VAT", cancellationToken);
        var pit = await _taxConfigRepository.GetByNameAsync("PIT", cancellationToken);

        // Warn admin when neither tax type is configured/enabled
        var vatMissing = vat is null || !vat.IsEnabled;
        var pitMissing = pit is null || !pit.IsEnabled;

        if (vatMissing && pitMissing)
            await _notificationService.NotifyAsync(
                "Chưa cấu hình thuế",
                "Cả VAT và PIT đều chưa được cấu hình hoặc đang tắt. " +
                "Hệ thống sẽ dùng tỷ lệ mặc định theo TT 40/2021 (VAT 1%, PIT 0.5%).",
                NotificationType.Warning,
                NotificationRole.Admin,
                cancellationToken: cancellationToken);

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