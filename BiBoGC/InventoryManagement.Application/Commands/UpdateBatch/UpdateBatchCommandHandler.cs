using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.UpdateBatch;

public class UpdateBatchCommandHandler : IRequestHandler<UpdateBatchCommand, Result<ProductBatchDto>>
{
    private readonly IProductBatchRepository _productBatchRepository;
    private readonly IProductRepository _productRepository;

    public UpdateBatchCommandHandler(
        IProductBatchRepository productBatchRepository,
        IProductRepository productRepository)
    {
        _productBatchRepository = productBatchRepository;
        _productRepository = productRepository;
    }

    public async Task<Result<ProductBatchDto>> Handle(UpdateBatchCommand request, CancellationToken cancellationToken)
    {
        // Get product with batches
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result<ProductBatchDto>.Failure($"Không tìm thấy sản phẩm với ID '{request.ProductId}'.");

        var batch = product.Batches.FirstOrDefault(b => b.Id == request.Id);
        if (batch is null)
            return Result<ProductBatchDto>.Failure($"Không tìm thấy lô hàng với ID '{request.Id}' cho sản phẩm này.");

        // Update quantity if provided
        if (request.Quantity.HasValue)
        {
            var currentQuantity = batch.Quantity;
            var newQuantity = request.Quantity.Value;

            if (newQuantity > currentQuantity)
                batch.IncreaseQuantity(newQuantity - currentQuantity);
            else if (newQuantity < currentQuantity) batch.DecreaseQuantity(currentQuantity - newQuantity);
        }

        // Note: ManufacturingDate, ExpirationDate, and CostPrice would need
        // domain methods to update them if needed. For now, we'll save via repository.

        await _productRepository.UpdateAsync(product, cancellationToken);

        var dto = ProductBatchDto.FromEntity(batch);

        return Result<ProductBatchDto>.Success(dto);
    }
}