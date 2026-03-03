using MediatR;
using Sale.Application.DTOs;
using Sale.Application.Interfaces;
using Sale.Application.Mappers;
using Shared.Application.Common;

namespace Sale.Application.Queries.GetInvoice;

public class GetInvoiceQueryHandler : IRequestHandler<GetInvoiceQuery, Result<InvoiceDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoiceQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Result<InvoiceDto>> Handle(
        GetInvoiceQuery request,
        CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdWithItemsAsync(request.InvoiceId, cancellationToken);
        if (invoice is null)
            return Result<InvoiceDto>.Failure($"Không tìm thấy hóa đơn với ID '{request.InvoiceId}'.");

        return Result<InvoiceDto>.Success(InvoiceMapper.MapToDto(invoice));
    }
}