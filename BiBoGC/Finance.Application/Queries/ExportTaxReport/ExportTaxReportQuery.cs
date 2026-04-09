using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.ExportTaxReport;

/// <param name="Year">Năm kê khai</param>
/// <param name="Month">Tháng kê khai (null = cả năm)</param>
public record ExportTaxReportQuery(int Year, int? Month) : IRequest<Result<ExportFileResult>>;