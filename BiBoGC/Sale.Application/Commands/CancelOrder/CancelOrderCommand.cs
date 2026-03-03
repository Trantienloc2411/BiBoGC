using MediatR;
using Sale.Application.DTOs;
using Shared.Application.Common;

namespace Sale.Application.Commands.CancelOrder;

public record CancelOrderCommand(
    Guid OrderId,
    string? Reason = null
) : IRequest<Result<SalesOrderDto>>;