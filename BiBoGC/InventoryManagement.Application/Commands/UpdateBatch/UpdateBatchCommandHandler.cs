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
            {
                batch.IncreaseQuantity(newQuantity - currentQuantity);
                product.IncreaseStock(newQuantity - currentQuantity);
            }
            else if (newQuantity < currentQuantity)
            {
                batch.DecreaseQuantity(currentQuantity - newQuantity);
                product.DecreaseStock(currentQuantity - newQuantity);
            }
        }

        if (request.ExpirationDate.HasValue && request.ExpirationDate.Value > DateTime.Now)
        {
            batch.UpdateExpirationDateBatch(request.ExpirationDate.Value);
        }
        else
        {
            return Result<ProductBatchDto>.Failure(
                $"Ngày hết hạn của lô hàng không được trong quá khứ hoặc ngày hiện tại. Ngày giờ hiện tại {DateTime.Now} - Ngày giờ cập nhật cho lô hàng {request.ExpirationDate.Value}. ");
        }
        
        if (request.ManufacturingDate.HasValue && request.ManufacturingDate.Value <= DateTime.Now)
        {
            batch.UpdateExpirationDateBatch(request.ExpirationDate.Value);
        }
        else
        {
            return Result<ProductBatchDto>.Failure(
                $"Ngày sản xất của lô hàng không được trong quá khứ. Ngày giờ hiện tại {DateTime.Now} - Ngày giờ cập nhật cho lô hàng {request.ManufacturingDate.Value}. ");
        }

        if (request.CostPrice is > 0)
        {
            batch.UpdateCostPrice(request.CostPrice.Value);
        }
        else
        {
            return Result<ProductBatchDto>.Failure(
                $"Giá nhập vào của lô không thể bằng 0 hoặc âm. Giá trị nhập vào {request.CostPrice.Value}");
        }
        

        await _productRepository.UpdateAsync(product, cancellationToken);

        var dto = ProductBatchDto.FromEntity(batch);

        return Result<ProductBatchDto>.Success(dto);
    }
}