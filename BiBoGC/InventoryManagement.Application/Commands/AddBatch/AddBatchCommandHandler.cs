using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using MediatR;
using Shared.Application.Common;
using Shared.Application.Interfaces;

namespace InventoryManagement.Application.Commands.AddBatch;

/// <summary>
/// Handler for AddBatchCommand
/// </summary>
public class AddBatchCommandHandler : IRequestHandler<AddBatchCommand, Result<ProductBatchDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductBatchRepository _productBatchesRepository; 


    public AddBatchCommandHandler(IProductRepository productRepository, IProductBatchRepository productBatchRepository)
    {
        _productRepository = productRepository;
        _productBatchesRepository = productBatchRepository;
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
            //var batch = product.AddNewBatch(
            //    batchNumber: request.BatchNumber,
            //    quantity: request.Quantity,
            //    manufacturingDate: DateTime.SpecifyKind(request.ManufacturingDate,DateTimeKind.Utc),
            //    expiryDate: DateTime.SpecifyKind(request.ExpirationDate, DateTimeKind.Utc),
            //    costPrice: request.CostPrice
            //);

            var productBatch = new ProductBatch(
                productId: request.ProductId,
                batchNumber: request.BatchNumber,
                quantity: request.Quantity,
                manufacturingDate: DateTime.SpecifyKind(request.ManufacturingDate, DateTimeKind.Utc),
                expirationDate: DateTime.SpecifyKind(request.ExpirationDate, DateTimeKind.Utc),
                costPrice: request.CostPrice
                );

            // Save changes
            var result = await _productBatchesRepository.AddAsync(productBatch, cancellationToken);

            // Sync TotalStock on the product to match the added batch quantity
            product.IncreaseStock(request.Quantity);
            await _productRepository.UpdateAsync(product, cancellationToken);

            // Return DTO
            var dto = ProductBatchDto.FromEntity(result);
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