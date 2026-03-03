using MediatR;
using Sale.Application.DTOs;
using Shared.Application.Common;

namespace Sale.Application.Queries.GetInvoices;

public record GetInvoicesQuery(
    int Page = 1,
    int PageSize = 20,
    DateTime? DateFrom = null,
    DateTime? DateTo = null
) : IRequest<Result<PagedResult<InvoiceDto>>>;