using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.ExportExistingProducts;

public record ExportExistingProductsQuery : IRequest<Result<ExportFileResult>>;
