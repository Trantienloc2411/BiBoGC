using Finance.Application.Interfaces;
using Finance.Application.Queries.GetDailySalesReport;
using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.ExportDailySalesReportPdf;

public class ExportDailySalesReportPdfQueryHandler : IRequestHandler<ExportDailySalesReportPdfQuery, Result<byte[]>>
{
    private readonly IMediator _mediator;
    private readonly IReportPdfExportService _pdfExportService;

    public ExportDailySalesReportPdfQueryHandler(IMediator mediator, IReportPdfExportService pdfExportService)
    {
        _mediator = mediator;
        _pdfExportService = pdfExportService;
    }

    public async Task<Result<byte[]>> Handle(ExportDailySalesReportPdfQuery request,
        CancellationToken cancellationToken)
    {
        var reportResult = await _mediator.Send(new GetDailySalesReportQuery(request.Date), cancellationToken);
        if (!reportResult.IsSuccess)
            return Result<byte[]>.Failure(reportResult.Errors);

        var pdfBytes = _pdfExportService.GenerateDailySalesReportPdf(reportResult.Value!);
        return Result<byte[]>.Success(pdfBytes);
    }
}