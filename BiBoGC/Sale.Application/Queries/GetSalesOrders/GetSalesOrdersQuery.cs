using MediatR;
using Sale.Application.DTOs;
using Sale.Domain.Enum;
using Shared.Application.Common;

namespace Sale.Application.Queries.GetSalesOrders;

public record GetSalesOrdersQuery(
    int Page = 1,
    int PageSize = 20,
    OrderStatus? Status = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    string? Search = null
) : IRequest<Result<PagedResult<SalesOrderDto>>>;