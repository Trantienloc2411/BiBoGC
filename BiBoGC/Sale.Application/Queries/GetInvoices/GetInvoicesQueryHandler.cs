using MediatR;
using Sale.Application.DTOs;
using Sale.Application.Interfaces;
using Sale.Application.Mappers;
using Shared.Application.Common;

namespace Sale.Application.Queries.GetInvoices;

public class GetInvoicesQueryHandler
    : IRequestHandler<GetInvoicesQuery, Result<PagedResult<InvoiceDto>>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoicesQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Result<PagedResult<InvoiceDto>>> Handle(
        GetInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        var (invoices, totalCount) = await _invoiceRepository.GetAllAsync(
            page: request.Page,
            pageSize: request.PageSize,
            dateFrom: request.DateFrom,
            dateTo: request.DateTo,
            ct: cancellationToken
        );

        var dtos = invoices.Select(InvoiceMapper.MapToDto);

        var pagedResult = new PagedResult<InvoiceDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageSize = request.PageSize,
            Page = request.Page
        };

        return Result<PagedResult<InvoiceDto>>.Success(pagedResult);
    }
}