using InventoryManagement.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.ExportExistingProducts;

public class ExportExistingProductsQueryHandler
    : IRequestHandler<ExportExistingProductsQuery, Result<ExportFileResult>>
{
    private readonly IProductExportService _exportService;

    public ExportExistingProductsQueryHandler(IProductExportService exportService)
    {
        _exportService = exportService;
    }

    public async Task<Result<ExportFileResult>> Handle(
        ExportExistingProductsQuery request,
        CancellationToken cancellationToken)
    {
        var exportResult = await _exportService.ExportExistingProductsAsync(cancellationToken);
        return Result<ExportFileResult>.Success(exportResult);
    }
}
