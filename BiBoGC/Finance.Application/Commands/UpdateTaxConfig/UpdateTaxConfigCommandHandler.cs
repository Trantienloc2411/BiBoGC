using Finance.Application.DTOs;
using Finance.Application.Interfaces;
using Finance.Domain.Entities;
using MediatR;
using Shared.Application.Common;
using Shared.Application.Interfaces;

namespace Finance.Application.Commands.UpdateTaxConfig;

public class UpdateTaxConfigCommandHandler : IRequestHandler<UpdateTaxConfigCommand, Result<TaxConfigDto>>
{
    private readonly IAuditLogger _auditLogger;
    private readonly ITaxConfigRepository _taxConfigRepository;
    private readonly IFinanceUnitOfWork _unitOfWork;

    public UpdateTaxConfigCommandHandler(
        ITaxConfigRepository taxConfigRepository,
        IFinanceUnitOfWork unitOfWork,
        IAuditLogger auditLogger)
    {
        _taxConfigRepository = taxConfigRepository;
        _unitOfWork = unitOfWork;
        _auditLogger = auditLogger;
    }

    public async Task<Result<TaxConfigDto>> Handle(UpdateTaxConfigCommand request, CancellationToken cancellationToken)
    {
        var taxType = request.TaxType.ToUpperInvariant();
        if (taxType is not ("VAT" or "PIT"))
            return Result<TaxConfigDto>.Failure("Loại thuế không hợp lệ. Giá trị hợp lệ: VAT, PIT.");

        var config = await _taxConfigRepository.GetByNameAsync(taxType, cancellationToken);

        if (config is null)
        {
            config = TaxConfiguration.Create(taxType, request.Rate, request.IsEnabled);
            await _taxConfigRepository.AddAsync(config, cancellationToken);
        }
        else
        {
            config.Update(request.Rate, request.IsEnabled);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "TaxConfig.Update",
            isSuccess: true,
            description: $"Type={taxType}, Rate={request.Rate:P2}, IsEnabled={request.IsEnabled}",
            cancellationToken: cancellationToken);

        return Result<TaxConfigDto>.Success(new TaxConfigDto
        {
            Id = config.Id,
            Name = config.Name,
            Rate = config.Rate,
            IsEnabled = config.IsEnabled,
            EffectiveFrom = config.EffectiveFrom,
            UpdatedAt = config.UpdatedAt ?? config.CreatedAt
        });
    }
}