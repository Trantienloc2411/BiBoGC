using InventoryManagement.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.DeleteProduct;

/// <summary>
/// Handler for DeleteProductCommand
/// </summary>
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IProductRepository _productRepository;

    public DeleteProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdWithoutBatchesAsync(request.Id, cancellationToken);
        if (product == null)
        {
            return Result.Failure($"Không tìm thấy sản phẩm với ID '{request.Id}'.");
        }

        await _productRepository.DeleteAsync(product, cancellationToken);

        return Result.Success();
    }
}
