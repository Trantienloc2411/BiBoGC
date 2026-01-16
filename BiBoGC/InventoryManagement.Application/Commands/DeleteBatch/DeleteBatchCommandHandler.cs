using InventoryManagement.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.DeleteBatch;

public class DeleteBatchCommandHandler : IRequestHandler<DeleteBatchCommand, Result>
{
    private readonly IProductBatchRepository _productBatchRepository;
    private readonly IProductRepository _productRepository;

    public DeleteBatchCommandHandler(
        IProductBatchRepository productBatchRepository,
        IProductRepository productRepository)
    {
        _productBatchRepository = productBatchRepository;
        _productRepository = productRepository;
    }

    public async Task<Result> Handle(DeleteBatchCommand request, CancellationToken cancellationToken)
    {
        // Verify product exists
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure($"Không tìm thấy sản phẩm với ID '{request.ProductId}'.");
        }

        // Verify batch belongs to product
        var batch = product.Batches.FirstOrDefault(b => b.Id == request.Id);
        if (batch is null)
        {
            return Result.Failure($"Không tìm thấy lô hàng với ID '{request.Id}' cho sản phẩm này.");
        }

        // Check if batch has quantity - warn if deleting batch with stock
        if (batch.Quantity > 0)
        {
            // We still allow deletion but it could be enhanced to require confirmation
            // For now, we'll proceed with the deletion
        }

        await _productBatchRepository.DeleteAsync(batch, cancellationToken);

        return Result.Success();
    }
}
