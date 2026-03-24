using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.ExportDailySalesReportPdf;

public record ExportDailySalesReportPdfQuery(DateOnly Date) : IRequest<Result<byte[]>>;