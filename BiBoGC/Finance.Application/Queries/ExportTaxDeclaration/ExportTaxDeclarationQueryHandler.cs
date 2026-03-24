using Finance.Application.Interfaces;
using MediatR;
using Shared.Application.Common;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Finance.Application.Queries.ExportTaxDeclaration;

public class ExportTaxDeclarationQueryHandler : IRequestHandler<ExportTaxDeclarationQuery, Result<byte[]>>
{
    private readonly ITaxDeclarationExportService _exportService;
    private readonly INotificationService _notificationService;
    private readonly ISalesDataReader _salesDataReader;
    private readonly ITaxConfigService _taxConfigService;

    public ExportTaxDeclarationQueryHandler(
        ISalesDataReader salesDataReader,
        ITaxDeclarationExportService exportService,
        ITaxConfigService taxConfigService,
        INotificationService notificationService)
    {
        _salesDataReader = salesDataReader;
        _exportService = exportService;
        _taxConfigService = taxConfigService;
        _notificationService = notificationService;
    }

    public async Task<Result<byte[]>> Handle(
        ExportTaxDeclarationQuery request,
        CancellationToken cancellationToken)
    {
        if (request.MonthFrom < 1 || request.MonthFrom > 12)
        {
            await _notificationService.NotifyAsync(
                "Xuất tờ khai thuế thất bại",
                $"Tháng bắt đầu ({request.MonthFrom}) không hợp lệ.",
                NotificationType.Error,
                NotificationRole.Admin,
                cancellationToken: cancellationToken);
            return Result<byte[]>.Failure("Tháng bắt đầu không hợp lệ (1–12).");
        }

        if (request.MonthTo < request.MonthFrom || request.MonthTo > 12)
        {
            await _notificationService.NotifyAsync(
                "Xuất tờ khai thuế thất bại",
                $"Tháng kết thúc ({request.MonthTo}) không hợp lệ hoặc nhỏ hơn tháng bắt đầu.",
                NotificationType.Error,
                NotificationRole.Admin,
                cancellationToken: cancellationToken);
            return Result<byte[]>.Failure("Tháng kết thúc không hợp lệ hoặc nhỏ hơn tháng bắt đầu.");
        }

        // Fetch aggregated revenue for the period
        var utcFrom = new DateTime(request.Year, request.MonthFrom, 1, 0, 0, 0, DateTimeKind.Utc);
        var utcTo = new DateTime(request.Year, request.MonthTo, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMonths(1).AddTicks(-1);

        var aggregate = await _salesDataReader.GetAggregateAsync(utcFrom, utcTo, cancellationToken);
        var revenue = aggregate.TotalRevenue;

        // Warn when no revenue data exists for the period
        if (revenue == 0m)
            await _notificationService.NotifyAsync(
                "Báo cáo thuế: không có dữ liệu",
                $"Không có doanh thu trong kỳ T{request.MonthFrom}–T{request.MonthTo}/{request.Year}. Tờ khai sẽ có giá trị 0đ.",
                NotificationType.Warning,
                NotificationRole.Admin,
                cancellationToken: cancellationToken);

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

        await _notificationService.NotifyAsync(
            "Xuất tờ khai thuế thành công",
            $"Tờ khai thuế 01/CNKD kỳ T{request.MonthFrom}–T{request.MonthTo}/{request.Year} đã được xuất. Doanh thu: {revenue:N0}đ.",
            NotificationType.Info,
            NotificationRole.Admin,
            cancellationToken: cancellationToken);

        return Result<byte[]>.Success(docxBytes);
    }
}