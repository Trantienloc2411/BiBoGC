using MediatR;
using Sale.Application.DTOs;
using Shared.Application.Common;

namespace Sale.Application.Commands.AddItemToOrder;

public record AddItemToOrderCommand(
    Guid OrderId,
    Guid ProductId,
    Guid ProductVariantId,
    Guid? ProductBatchId,
    int Quantity
) : IRequest<Result<SalesOrderDto>>;