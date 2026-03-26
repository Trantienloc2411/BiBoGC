using MediatR;
using Sale.Application.Interfaces;
using Sale.Application.Mappers;
using Shared.Application.Common;

namespace Sale.Application.Queries.ExportInvoicePdf;

public class ExportInvoicePdfQueryHandler : IRequestHandler<ExportInvoicePdfQuery, Result<byte[]>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPdfExportService _pdfExportService;

    public ExportInvoicePdfQueryHandler(
        IInvoiceRepository invoiceRepository,
        IPdfExportService pdfExportService)
    {
        _invoiceRepository = invoiceRepository;
        _pdfExportService = pdfExportService;
    }

    public async Task<Result<byte[]>> Handle(ExportInvoicePdfQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdWithItemsAsync(request.InvoiceId, cancellationToken);

        if (invoice is null)
            return Result<byte[]>.Failure($"Không tìm thấy hóa đơn với ID '{request.InvoiceId}'.");

        var dto = InvoiceMapper.MapToDto(invoice);
        var pdfBytes = _pdfExportService.GenerateInvoicePdf(dto);

        return Result<byte[]>.Success(pdfBytes);
    }
}