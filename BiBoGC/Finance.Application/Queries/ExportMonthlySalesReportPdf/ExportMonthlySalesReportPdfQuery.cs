using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.ExportMonthlySalesReportPdf;

public record ExportMonthlySalesReportPdfQuery(int Year, int Month) : IRequest<Result<byte[]>>;