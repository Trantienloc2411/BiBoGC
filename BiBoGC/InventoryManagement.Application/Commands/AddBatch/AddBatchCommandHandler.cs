using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.AddBatch;

/// <summary>
/// Handler for AddBatchCommand
/// </summary>
public class AddBatchCommandHandler : IRequestHandler<AddBatchCommand, Result<ProductBatchDto>>
{
    private readonly IProductRepository _productRepository;

    public AddBatchCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductBatchDto>> Handle(AddBatchCommand request, CancellationToken cancellationToken)
    {
        // Get product with batches
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            return Result<ProductBatchDto>.Failure($"Không tìm thấy sản phẩm với ID '{request.ProductId}'.");

        try
        {
            // Add new batch using domain logic
            var batch = product.AddNewBatch(
                request.BatchNumber,
                request.Quantity,
                request.ManufacturingDate,
                request.ExpirationDate,
                request.CostPrice
            );

            // Save changes
            await _productRepository.UpdateAsync(product, cancellationToken);

            // Return DTO
            var dto = ProductBatchDto.FromEntity(batch);
            return Result<ProductBatchDto>.Success(dto);
        }
        catch (InvalidOperationException ex)
        {
            return Result<ProductBatchDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result<ProductBatchDto>.Failure(ex.Message);
        }
    }
}