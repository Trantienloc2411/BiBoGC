using MediatR;
using Sale.Application.DTOs;
using Shared.Application.Common;

namespace Sale.Application.Queries.GetInvoice;

public record GetInvoiceQuery(Guid InvoiceId) : IRequest<Result<InvoiceDto>>;