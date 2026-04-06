using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetProductVariantsByProductId;

public class GetProductVariantsByProductIdQueryHandler(IProductVariantRepository _productVariantRepository) :
    IRequestHandler<GetProductVariantsByProductIdQuery, Result<IEnumerable<ProductVariantDto>>>
{
    public async Task<Result<IEnumerable<ProductVariantDto>>> Handle(GetProductVariantsByProductIdQuery request,
        CancellationToken cancellationToken)
    {
        var productVariant =
            await _productVariantRepository.GetProductVariantsByProductIdAsync(request.ProductId, cancellationToken);
        var productVariants = productVariant.ToList();
        if (productVariants.Count == 0)
            return Result<IEnumerable<ProductVariantDto>>.Failure($"Hiện tại sản phẩm chưa có biến thể nào.");

        var result = productVariants.Select(MapToDto);
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
            Sku = productVariant.SkuUnique.Value,
            VariantName = productVariant.VariantName,
            Unit = productVariant.Unit,
            QuantityBaseUnit = productVariant.QuantityBaseUnit,
            CostPrice = productVariant.CostPrice?.Value,
            SalePrice = productVariant.SalePrice.Value,
            DisplayOrder = productVariant.DisplayOrder,
            CreatedAt = productVariant.CreatedAt,
            UpdatedAt = productVariant.UpdatedAt,
            UnitName = productVariant.Unit.ToString()
        };
    }
}