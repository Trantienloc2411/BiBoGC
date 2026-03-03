using MediatR;
using Sale.Application.DTOs;
using Shared.Application.Common;

namespace Sale.Application.Commands.CompleteOrder;

public record CompleteOrderCommand(
    Guid OrderId,
    decimal AmountPaid
) : IRequest<Result<SalesOrderDto>>;