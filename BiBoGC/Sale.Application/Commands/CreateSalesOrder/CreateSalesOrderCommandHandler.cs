using MediatR;
using Sale.Application.DTOs;
using Sale.Application.Interfaces;
using Sale.Application.Mappers;
using Sale.Domain.Domain;
using Shared.Application.Common;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Sale.Application.Commands.CreateSalesOrder;

public class CreateSalesOrderCommandHandler : IRequestHandler<CreateSalesOrderCommand, Result<SalesOrderDto>>
{
    private readonly IOrderNumberGenerator _orderNumberGenerator;
    private readonly ISalesOrderRepository _orderRepository;
    private readonly INotificationService _notificationService;

    public CreateSalesOrderCommandHandler(
        ISalesOrderRepository orderRepository,
        IOrderNumberGenerator orderNumberGenerator,
        INotificationService notificationService)
    {
        _orderRepository = orderRepository;
        _orderNumberGenerator = orderNumberGenerator;
        _notificationService = notificationService;
    }

    public async Task<Result<SalesOrderDto>> Handle(
        CreateSalesOrderCommand request,
        CancellationToken cancellationToken)
    {
        var orderNumber = await _orderNumberGenerator.GenerateNextAsync(cancellationToken);

        var order = new SalesOrder(
            orderNumber: orderNumber,
            paymentMethod: request.PaymentMethod,
            customerName: request.CustomerName,
            customerPhone: request.CustomerPhone,
            notes: request.Notes
        );

        await _orderRepository.AddAsync(order, cancellationToken);

        await _notificationService.NotifyAsync(
            "Đơn hàng mới",
            $"Đơn hàng {order.OrderNumber} vừa được tạo.",
            NotificationType.NewOrder,
            NotificationRole.Both,
            order.Id,
            "SalesOrder",
            cancellationToken);

        return Result<SalesOrderDto>.Success(SalesOrderMapper.MapToDto(order));
    }
}