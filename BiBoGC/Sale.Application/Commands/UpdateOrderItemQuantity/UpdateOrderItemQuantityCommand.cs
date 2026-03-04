using MediatR;
using Sale.Application.DTOs;
using Shared.Application.Common;

namespace Sale.Application.Commands.UpdateOrderItemQuantity;

public record UpdateOrderItemQuantityCommand(
    Guid OrderId,
    Guid ItemId,
    int NewQuantity
) : IRequest<Result<SalesOrderDto>>;