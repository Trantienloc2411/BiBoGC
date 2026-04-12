using System.IO.Compression;
using MediatR;
using Sale.Application.Interfaces;
using Sale.Application.Mappers;
using Sale.Domain.Domain;
using Shared.Application.Common;

namespace Sale.Application.Queries.ExportInvoicesZip;

public class ExportInvoicesZipQueryHandler : IRequestHandler<ExportInvoicesZipQuery, Result<byte[]>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ISalesOrderRepository _orderRepository;
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
    private readonly IStoreInfoService _storeInfoService;
    private readonly ISaleUnitOfWork _unitOfWork;
    private readonly IPdfExportService _pdfExportService;

    public ExportInvoicesZipQueryHandler(
        IInvoiceRepository invoiceRepository,
        ISalesOrderRepository orderRepository,
        IInvoiceNumberGenerator invoiceNumberGenerator,
        IStoreInfoService storeInfoService,
        ISaleUnitOfWork unitOfWork,
        IPdfExportService pdfExportService)
    {
        _invoiceRepository = invoiceRepository;
        _orderRepository = orderRepository;
        _invoiceNumberGenerator = invoiceNumberGenerator;
        _storeInfoService = storeInfoService;
        _unitOfWork = unitOfWork;
        _pdfExportService = pdfExportService;
    }

    public async Task<Result<byte[]>> Handle(ExportInvoicesZipQuery request, CancellationToken cancellationToken)
    {
        // 1. Auto-create invoices for completed orders that don't have one yet
        var ordersWithoutInvoice = await _orderRepository.GetCompletedWithoutInvoiceAsync(
            request.DateFrom, request.DateTo, cancellationToken);

        if (ordersWithoutInvoice.Any())
        {
            var storeInfo = await _storeInfoService.GetCurrentAsync(cancellationToken);

            foreach (var order in ordersWithoutInvoice)
            {
                var invoiceNumber = await _invoiceNumberGenerator.GenerateNextAsync(cancellationToken);
                var invoice = Invoice.CreateFromOrder(order, storeInfo, invoiceNumber);

                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    await _invoiceRepository.AddAsync(invoice, cancellationToken);
                    order.SetInvoiceId(invoice.Id);
                    await _orderRepository.UpdateAsync(order, cancellationToken);
                }, cancellationToken);
            }
        }

        // 2. Fetch all invoices in the date range (now includes newly created ones)
        var invoices = await _invoiceRepository.GetAllForExportAsync(
            request.DateFrom, request.DateTo, cancellationToken);

        var invoiceList = invoices.ToList();
        if (invoiceList.Count == 0)
            return Result<byte[]>.Failure("Không có hóa đơn nào trong khoảng thời gian đã chọn.");

        // 3. Build ZIP archive grouped by date folder (d-M-yyyy in local time)
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var invoice in invoiceList)
            {
                var localDate = invoice.InvoiceDate.ToLocalTime();
                var folderName = $"{localDate.Day}-{localDate.Month}-{localDate.Year}";
                var fileName = $"HD-{invoice.InvoiceNumber}.pdf";
                var entryPath = $"{folderName}/{fileName}";

                var dto = InvoiceMapper.MapToDto(invoice);
                var pdfBytes = _pdfExportService.GenerateInvoicePdf(dto);

                var entry = archive.CreateEntry(entryPath, CompressionLevel.Optimal);
                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(pdfBytes, cancellationToken);
            }
        }

        return Result<byte[]>.Success(memoryStream.ToArray());
    }
}
