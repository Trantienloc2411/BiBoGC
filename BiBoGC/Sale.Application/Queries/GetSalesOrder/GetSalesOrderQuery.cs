using MediatR;
using Sale.Application.DTOs;
using Shared.Application.Common;

namespace Sale.Application.Queries.GetSalesOrder;

public record GetSalesOrderQuery(Guid OrderId) : IRequest<Result<SalesOrderDto>>;