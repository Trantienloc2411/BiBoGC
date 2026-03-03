using MediatR;
using Sale.Application.DTOs;
using Sale.Application.Interfaces;
using Sale.Application.Mappers;
using Shared.Application.Common;

namespace Sale.Application.Queries.GetSalesOrder;

public class GetSalesOrderQueryHandler : IRequestHandler<GetSalesOrderQuery, Result<SalesOrderDto>>
{
    private readonly ISalesOrderRepository _orderRepository;

    public GetSalesOrderQueryHandler(ISalesOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<SalesOrderDto>> Handle(
        GetSalesOrderQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result<SalesOrderDto>.Failure($"Không tìm thấy đơn hàng với ID '{request.OrderId}'.");

        return Result<SalesOrderDto>.Success(SalesOrderMapper.MapToDto(order));
    }
}