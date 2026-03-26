using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.ExportTaxDeclaration;

/// <param name="Year">Năm kê khai</param>
/// <param name="MonthFrom">Tháng bắt đầu (1-12)</param>
/// <param name="MonthTo">Tháng kết thúc (1-12)</param>
public record ExportTaxDeclarationQuery(int Year, int MonthFrom, int MonthTo)
    : IRequest<Result<byte[]>>;