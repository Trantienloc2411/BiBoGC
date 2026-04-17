using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.ImportProducts;

public class ImportProductsCommandHandler
    : IRequestHandler<ImportProductsCommand, Result<ImportProductsResultDto>>
{
    private readonly IProductImportService _importService;

    public ImportProductsCommandHandler(IProductImportService importService)
    {
        _importService = importService;
    }

    public Task<Result<ImportProductsResultDto>> Handle(
        ImportProductsCommand request,
        CancellationToken cancellationToken)
        => _importService.ImportFromExcelAsync(request.FileStream, request.DryRun, cancellationToken);
}
