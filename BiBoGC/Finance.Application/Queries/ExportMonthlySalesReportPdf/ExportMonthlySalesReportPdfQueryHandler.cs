using Finance.Application.Interfaces;
using Finance.Application.Queries.GetMonthlySalesReport;
using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.ExportMonthlySalesReportPdf;

public class ExportMonthlySalesReportPdfQueryHandler
    : IRequestHandler<ExportMonthlySalesReportPdfQuery, Result<byte[]>>
{
    private readonly IMediator _mediator;
    private readonly IReportPdfExportService _pdfExportService;

    public ExportMonthlySalesReportPdfQueryHandler(IMediator mediator, IReportPdfExportService pdfExportService)
    {
        _mediator = mediator;
        _pdfExportService = pdfExportService;
    }

    public async Task<Result<byte[]>> Handle(
        ExportMonthlySalesReportPdfQuery request, CancellationToken cancellationToken)
    {
        var reportResult = await _mediator.Send(
            new GetMonthlySalesReportQuery(request.Year, request.Month), cancellationToken);

        if (!reportResult.IsSuccess)
            return Result<byte[]>.Failure(reportResult.Errors);

        var pdfBytes = _pdfExportService.GenerateMonthlySalesReportPdf(reportResult.Value!);
        return Result<byte[]>.Success(pdfBytes);
    }
}