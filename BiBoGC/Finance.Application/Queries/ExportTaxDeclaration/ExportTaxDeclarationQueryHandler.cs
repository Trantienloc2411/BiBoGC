using Finance.Application.Interfaces;
using MediatR;
using Shared.Application.Common;
using Shared.Application.Interfaces;

namespace Finance.Application.Queries.ExportTaxDeclaration;

public class ExportTaxDeclarationQueryHandler : IRequestHandler<ExportTaxDeclarationQuery, Result<byte[]>>
{
    private readonly ITaxDeclarationExportService _exportService;
    private readonly ISalesDataReader _salesDataReader;
    private readonly ITaxConfigService _taxConfigService;

    public ExportTaxDeclarationQueryHandler(
        ISalesDataReader salesDataReader,
        ITaxDeclarationExportService exportService,
        ITaxConfigService taxConfigService)
    {
        _salesDataReader = salesDataReader;
        _exportService = exportService;
        _taxConfigService = taxConfigService;
    }

    public async Task<Result<byte[]>> Handle(
        ExportTaxDeclarationQuery request,
        CancellationToken cancellationToken)
    {
        if (request.MonthFrom < 1 || request.MonthFrom > 12)
            return Result<byte[]>.Failure("Tháng bắt đầu không hợp lệ (1–12).");

        if (request.MonthTo < request.MonthFrom || request.MonthTo > 12)
            return Result<byte[]>.Failure("Tháng kết thúc không hợp lệ hoặc nhỏ hơn tháng bắt đầu.");

        // Fetch aggregated revenue for the period
        var utcFrom = new DateTime(request.Year, request.MonthFrom, 1, 0, 0, 0, DateTimeKind.Utc);
        var utcTo = new DateTime(request.Year, request.MonthTo, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMonths(1).AddTicks(-1);

        var aggregate = await _salesDataReader.GetAggregateAsync(utcFrom, utcTo, cancellationToken);
        var revenue = aggregate.TotalRevenue;

        // Use configured rates; fall back to statutory defaults when not yet configured
        var gtgtRate = await _taxConfigService.GetActiveVatRateAsync(cancellationToken);
        var tncnRate = await _taxConfigService.GetActivePitRateAsync(cancellationToken);
        if (gtgtRate == 0m) gtgtRate = 0.01m; // TT 40/2021/TT-BTC default: 1%
        if (tncnRate == 0m) tncnRate = 0.005m; // TT 40/2021/TT-BTC default: 0.5%

        var today = DateTime.UtcNow;

        var data = new TaxDeclarationData(
            CurrentYear: request.Year,
            MonthFrom: request.MonthFrom,
            YearFrom: request.Year,
            MonthTo: request.MonthTo,
            YearTo: request.Year,
            Revenue: revenue,
            GtgtTaxAmount: Math.Round(revenue * gtgtRate, 0, MidpointRounding.AwayFromZero),
            TncnTaxAmount: Math.Round(revenue * tncnRate, 0, MidpointRounding.AwayFromZero),
            DayExport: today.Day,
            MonthExport: today.Month,
            YearExport: today.Year);

        var docxBytes = _exportService.GenerateTaxDeclarationDocx(data);
        return Result<byte[]>.Success(docxBytes);
    }
}