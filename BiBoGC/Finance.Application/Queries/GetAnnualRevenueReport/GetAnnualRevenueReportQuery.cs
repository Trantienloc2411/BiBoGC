using Finance.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.GetAnnualRevenueReport;

public record GetAnnualRevenueReportQuery(int Year) : IRequest<Result<AnnualRevenueReportDto>>;