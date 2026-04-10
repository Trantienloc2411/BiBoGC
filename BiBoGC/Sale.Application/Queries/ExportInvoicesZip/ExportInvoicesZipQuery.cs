using MediatR;
using Shared.Application.Common;

namespace Sale.Application.Queries.ExportInvoicesZip;

public record ExportInvoicesZipQuery(
    DateTime? DateFrom = null,
    DateTime? DateTo = null
) : IRequest<Result<byte[]>>;
