using Finance.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.GetTaxConfig;

public record GetTaxConfigQuery : IRequest<Result<TaxConfigsDto>>;