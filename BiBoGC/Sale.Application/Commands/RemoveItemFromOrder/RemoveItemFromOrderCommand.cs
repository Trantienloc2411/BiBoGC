using MediatR;
using Sale.Application.DTOs;
using Shared.Application.Common;

namespace Sale.Application.Commands.RemoveItemFromOrder;

public record RemoveItemFromOrderCommand(
    Guid OrderId,
    Guid ItemId
) : IRequest<Result<SalesOrderDto>>;