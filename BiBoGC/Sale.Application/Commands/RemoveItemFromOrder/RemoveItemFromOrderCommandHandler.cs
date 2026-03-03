using MediatR;
using Sale.Application.DTOs;
using Sale.Application.Interfaces;
using Sale.Application.Mappers;
using Shared.Application.Common;

namespace Sale.Application.Commands.RemoveItemFromOrder;

public class RemoveItemFromOrderCommandHandler
    : IRequestHandler<RemoveItemFromOrderCommand, Result<SalesOrderDto>>
{
    private readonly ISalesOrderRepository _orderRepository;

    public RemoveItemFromOrderCommandHandler(ISalesOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<SalesOrderDto>> Handle(
        RemoveItemFromOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result<SalesOrderDto>.Failure($"Không tìm thấy đơn hàng với ID '{request.OrderId}'.");

        try
        {
            order.RemoveItem(request.ItemId);
        }
        catch (InvalidOperationException ex)
        {
            return Result<SalesOrderDto>.Failure(ex.Message);
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);

        return Result<SalesOrderDto>.Success(SalesOrderMapper.MapToDto(order));
    }
}