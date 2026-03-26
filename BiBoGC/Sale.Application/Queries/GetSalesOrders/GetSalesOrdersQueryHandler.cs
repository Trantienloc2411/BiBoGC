using MediatR;
using Sale.Application.DTOs;
using Sale.Application.Interfaces;
using Sale.Application.Mappers;
using Shared.Application.Common;

namespace Sale.Application.Queries.GetSalesOrders;

public class GetSalesOrdersQueryHandler
    : IRequestHandler<GetSalesOrdersQuery, Result<PagedResult<SalesOrderDto>>>
{
    private readonly ISalesOrderRepository _orderRepository;

    public GetSalesOrdersQueryHandler(ISalesOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<PagedResult<SalesOrderDto>>> Handle(
        GetSalesOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var (orders, totalCount, invoiceNumbers) = await _orderRepository.GetAllAsync(
            page: request.Page,
            pageSize: request.PageSize,
            status: request.Status,
            dateFrom: request.DateFrom,
            dateTo: request.DateTo,
            search: request.Search,
            ct: cancellationToken
        );

        var dtos = orders.Select(o =>
        {
            var dto = SalesOrderMapper.MapToDto(o);
            if (o.InvoiceId.HasValue && invoiceNumbers.TryGetValue(o.InvoiceId.Value, out var num))
                dto.InvoiceNumber = num;
            return dto;
        });

        var pagedResult = new PagedResult<SalesOrderDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageSize = request.PageSize,
            Page = request.Page
        };

        return Result<PagedResult<SalesOrderDto>>.Success(pagedResult);
    }
}