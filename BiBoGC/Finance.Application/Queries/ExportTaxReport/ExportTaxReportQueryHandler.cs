using Finance.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.ExportTaxReport;

public class ExportTaxReportQueryHandler : IRequestHandler<ExportTaxReportQuery, Result<ExportFileResult>>
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

    public async Task<Result<ExportFileResult>> Handle(ExportTaxReportQuery request, CancellationToken cancellationToken)
    {
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

        var invoices = await _invoiceDataReader.GetInvoicesForTaxReportAsync(utcFrom, utcTo, cancellationToken);

        if (invoices.Count == 0)
            return Result<ExportFileResult>.Failure("Không có hoá đơn nào trong kỳ kê khai này.");

        int fromMonth = request.Month ?? 1;
        int toMonth = request.Month ?? 12;

        var exportResult = _exportService.GenerateTaxReportExcel(
            fromMonth, request.Year,
            toMonth, request.Year,
            invoices);

        return Result<ExportFileResult>.Success(exportResult);
    }
}
