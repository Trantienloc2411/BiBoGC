using MediatR;
using Sale.Application.DTOs;
using Sale.Application.Interfaces;
using Sale.Application.Mappers;
using Shared.Application.Common;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Sale.Application.Commands.CancelOrder;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<SalesOrderDto>>
{
    private readonly ISalesOrderRepository _orderRepository;
    private readonly INotificationService _notificationService;

    public CancelOrderCommandHandler(
        ISalesOrderRepository orderRepository,
        INotificationService notificationService)
    {
        _orderRepository = orderRepository;
        _notificationService = notificationService;
    }

    public async Task<Result<SalesOrderDto>> Handle(
        CancelOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result<SalesOrderDto>.Failure($"Không tìm thấy đơn hàng với ID '{request.OrderId}'.");

        try
        {
            order.Cancel(request.Reason);
        }
        catch (InvalidOperationException ex)
        {
            return Result<SalesOrderDto>.Failure(ex.Message);
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);

        var reasonText = string.IsNullOrWhiteSpace(request.Reason) ? "" : $" Lý do: {request.Reason}";
        await _notificationService.NotifyAsync(
            "Đơn hàng bị huỷ",
            $"Đơn hàng {order.OrderNumber} đã bị huỷ.{reasonText}",
            NotificationType.OrderCancelled,
            NotificationRole.Both,
            order.Id,
            "SalesOrder",
            cancellationToken);

        return Result<SalesOrderDto>.Success(SalesOrderMapper.MapToDto(order));
    }
}