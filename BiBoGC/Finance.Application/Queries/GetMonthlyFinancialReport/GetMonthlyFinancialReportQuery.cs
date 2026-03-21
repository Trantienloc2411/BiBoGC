using Finance.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.GetMonthlyFinancialReport;

public record GetMonthlyFinancialReportQuery(int Year, int Month) : IRequest<Result<MonthlyFinancialReportDto>>;
