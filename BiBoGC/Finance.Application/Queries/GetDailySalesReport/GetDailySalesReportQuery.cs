using Finance.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.GetDailySalesReport;

public record GetDailySalesReportQuery(DateOnly Date) : IRequest<Result<DailySalesReportDto>>;
