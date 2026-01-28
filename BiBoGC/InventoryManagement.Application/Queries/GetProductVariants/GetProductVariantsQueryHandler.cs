using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetProductVariants;

public class GetProductVariantsQueryHandler(IProductVariantRepository productVariantRepository) :
    IRequestHandler<GetProductVariantsQuery, PagedResult<ProductVariantDto>>
{
    public async Task<PagedResult<ProductVariantDto>> Handle(GetProductVariantsQuery request,
        CancellationToken cancellationToken)
    {
        var (products, totalCount) = await productVariantRepository.GetAllAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchString,
            cancellationToken
        );

        var variantDtos = products.Select(MapToDto);

        return new PagedResult<ProductVariantDto>
        {
            Items = variantDtos,
            TotalCount = totalCount,
            PageSize = request.PageSize,
            Page = request.PageNumber
        };
    }

    private static ProductVariantDto MapToDto(ProductVariant productVariant)
    {
        return new ProductVariantDto
        {
            Id = productVariant.Id,
            Barcode = productVariant.Barcode,
            CostPrice = productVariant.CostPrice,
            CreatedAt = productVariant.CreatedAt,
            ProductName = productVariant.Product!.Name ?? string.Empty,
            ProductId = productVariant.Product.Id,
            QuantityBaseUnit = productVariant.QuantityBaseUnit,
            SalePrice = productVariant.SalePrice,
            SkuUnique = productVariant.SkuUnique,
            VariantName = productVariant.VariantName,
            Unit = productVariant.Unit
        };
    }
}