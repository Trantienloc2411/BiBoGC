using MediatR;
using Sale.Application.DTOs;
using Shared.Application.Common;

namespace Sale.Application.Commands.GenerateInvoice;

public record GenerateInvoiceCommand(
    Guid OrderId
) : IRequest<Result<InvoiceDto>>;