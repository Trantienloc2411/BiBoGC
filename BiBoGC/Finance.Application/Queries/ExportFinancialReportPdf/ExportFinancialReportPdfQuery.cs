using MediatR;
using Shared.Application.Common;

namespace Finance.Application.Queries.ExportFinancialReportPdf;

public record ExportFinancialReportPdfQuery(int Year, int Month) : IRequest<Result<byte[]>>;