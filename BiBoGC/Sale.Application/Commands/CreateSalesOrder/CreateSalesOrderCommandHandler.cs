using MediatR;
using Sale.Application.DTOs;
using Sale.Application.Interfaces;
using Sale.Application.Mappers;
using Sale.Domain.Domain;
using Shared.Application.Common;

namespace Sale.Application.Commands.CreateSalesOrder;

public class CreateSalesOrderCommandHandler : IRequestHandler<CreateSalesOrderCommand, Result<SalesOrderDto>>
{
    private readonly IOrderNumberGenerator _orderNumberGenerator;
    private readonly ISalesOrderRepository _orderRepository;

    public CreateSalesOrderCommandHandler(
        ISalesOrderRepository orderRepository,
        IOrderNumberGenerator orderNumberGenerator)
    {
        _orderRepository = orderRepository;
        _orderNumberGenerator = orderNumberGenerator;
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

        return Result<SalesOrderDto>.Success(SalesOrderMapper.MapToDto(order));
    }
}