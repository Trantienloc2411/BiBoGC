using Finance.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.GetDailyExpenseSummary;

public record GetDailyExpenseSummaryQuery(DateOnly Date) : IRequest<Result<DailyExpenseSummaryDto>>;
