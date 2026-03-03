using MediatR;
using Sale.Application.DTOs;
using Shared.Application.Common;

namespace Sale.Application.Commands.ApplyDiscount;

public record ApplyDiscountCommand(
    Guid OrderId,
    decimal DiscountAmount
) : IRequest<Result<SalesOrderDto>>;