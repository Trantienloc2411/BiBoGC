using Finance.Application.Interfaces;
using Finance.Application.Queries.GetMonthlyFinancialReport;
using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.ExportFinancialReportPdf;

public class ExportFinancialReportPdfQueryHandler : IRequestHandler<ExportFinancialReportPdfQuery, Result<byte[]>>
{
    private readonly IMediator _mediator;
    private readonly IReportPdfExportService _pdfExportService;

    public ExportFinancialReportPdfQueryHandler(IMediator mediator, IReportPdfExportService pdfExportService)
    {
        _mediator = mediator;
        _pdfExportService = pdfExportService;
    }

    public async Task<Result<byte[]>> Handle(
        ExportFinancialReportPdfQuery request, CancellationToken cancellationToken)
    {
        var reportResult = await _mediator.Send(
            new GetMonthlyFinancialReportQuery(request.Year, request.Month), cancellationToken);

        if (!reportResult.IsSuccess)
            return Result<byte[]>.Failure(reportResult.Errors);

        var pdfBytes = _pdfExportService.GenerateFinancialReportPdf(reportResult.Value!);
        return Result<byte[]>.Success(pdfBytes);
    }
}