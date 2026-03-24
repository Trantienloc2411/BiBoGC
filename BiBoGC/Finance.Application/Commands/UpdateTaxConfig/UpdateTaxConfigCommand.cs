using Finance.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Commands.UpdateTaxConfig;

/// <param name="TaxType">"VAT" or "PIT"</param>
public record UpdateTaxConfigCommand(string TaxType, decimal Rate, bool IsEnabled) : IRequest<Result<TaxConfigDto>>;