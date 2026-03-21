using Finance.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.GetMonthlySalesReport;

public record GetMonthlySalesReportQuery(int Year, int Month) : IRequest<Result<MonthlySalesReportDto>>;
