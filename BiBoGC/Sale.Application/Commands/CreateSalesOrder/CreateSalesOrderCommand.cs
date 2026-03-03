using MediatR;
using Sale.Application.DTOs;
using Sale.Domain.Enum;
using Shared.Application.Common;

namespace Sale.Application.Commands.CreateSalesOrder;

public record CreateSalesOrderCommand(
    PaymentMethod PaymentMethod,
    string? CustomerName = null,
    string? CustomerPhone = null,
    string? Notes = null
) : IRequest<Result<SalesOrderDto>>;