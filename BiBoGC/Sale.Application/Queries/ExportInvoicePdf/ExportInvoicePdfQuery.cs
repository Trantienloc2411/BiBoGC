using MediatR;
using Shared.Application.Common;

namespace Sale.Application.Queries.ExportInvoicePdf;

public record ExportInvoicePdfQuery(Guid InvoiceId) : IRequest<Result<byte[]>>;