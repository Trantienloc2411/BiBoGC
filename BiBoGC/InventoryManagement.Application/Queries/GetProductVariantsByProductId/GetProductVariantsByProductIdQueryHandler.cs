using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetProductVariantsByProductId;

public class GetProductVariantsByProductIdQueryHandler (IProductVariantRepository _productVariantRepository): 
    IRequestHandler<GetProductVariantsByProductIdQuery, Result<IEnumerable<ProductVariantDto>>>
{
    
    public async Task<Result<IEnumerable<ProductVariantDto>>> Handle(GetProductVariantsByProductIdQuery request, CancellationToken cancellationToken)
    {
        var productVariant =
            await _productVariantRepository.
                GetProductVariantsByProductIdAsync(request.ProductId, cancellationToken);
        if (productVariant is null)
            return Result<IEnumerable<ProductVariantDto>>.Failure($"Hiện tại sản phẩm chưa có biến thể nào.");
        
        var result = productVariant.Select(MapToDto);
        return Result<IEnumerable<ProductVariantDto>>.Success(result);
    }
    
    private static ProductVariantDto MapToDto(ProductVariant productVariant)
    {
        return new ProductVariantDto
        {
            Id = productVariant.Id,
            Barcode = productVariant.Barcode,
            ProductId = productVariant.ProductId,
            ProductName = productVariant.Product?.Name ?? string.Empty,
            SkuUnique = productVariant.SkuUnique,
            VariantName = productVariant.VariantName,
            Unit = productVariant.Unit,
            QuantityBaseUnit = productVariant.QuantityBaseUnit,
            CostPrice = productVariant.CostPrice,
            SalePrice = productVariant.SalePrice,
            DisplayOrder = productVariant.DisplayOrder,
            CreatedAt = productVariant.CreatedAt,
            UpdatedAt = productVariant.UpdatedAt
        };
    }
}