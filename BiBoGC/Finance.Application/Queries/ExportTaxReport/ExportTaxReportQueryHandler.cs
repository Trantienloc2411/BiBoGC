using Finance.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.ExportTaxReport;

public class ExportTaxReportQueryHandler : IRequestHandler<ExportTaxReportQuery, Result<byte[]>>
{
    private readonly ITaxReportExportService _exportService;
    private readonly IInvoiceDataReader _invoiceDataReader;
    private readonly ITaxConfigRepository _taxConfigRepository;

    public ExportTaxReportQueryHandler(
        IInvoiceDataReader invoiceDataReader,
        ITaxReportExportService exportService,
        ITaxConfigRepository taxConfigRepository)
    {
        _invoiceDataReader = invoiceDataReader;
        _exportService = exportService;
        _taxConfigRepository = taxConfigRepository;
    }

    public async Task<Result<byte[]>> Handle(ExportTaxReportQuery request, CancellationToken cancellationToken)
    {
        // Determine date range
        DateTime utcFrom, utcTo;
        if (request.Month.HasValue)
        {
            utcFrom = new DateTime(request.Year, request.Month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
            utcTo = utcFrom.AddMonths(1).AddTicks(-1);
        }
        else
        {
            utcFrom = new DateTime(request.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            utcTo = new DateTime(request.Year, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc);
        }

        // Get store tax code from config
        var taxConfig = await _taxConfigRepository.GetActiveAsync(cancellationToken);
        var storeTaxCode = taxConfig is not null ? "Xem cấu hình cửa hàng" : "Chưa cấu hình";

        // Fetch invoices
        var invoices = await _invoiceDataReader.GetInvoicesForTaxReportAsync(utcFrom, utcTo, cancellationToken);

        // Validate
        if (invoices.Count == 0)
            return Result<byte[]>.Failure("Không có hoá đơn nào trong kỳ kê khai này.");

        var bytes = _exportService.GenerateTaxReportExcel(
            request.Year, request.Month, storeTaxCode, invoices);

        return Result<byte[]>.Success(bytes);
    }
}