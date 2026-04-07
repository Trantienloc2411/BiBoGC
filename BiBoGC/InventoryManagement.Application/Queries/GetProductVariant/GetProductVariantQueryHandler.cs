using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.Queries.GetProductVariant;
using InventoryManagement.Domain.Entities;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetProducVariant;

public class GetProductVariantQueryHandler(IProductVariantRepository productVariantRepository)
    : IRequestHandler<GetProductVariantQuery, Result<ProductVariantDto>>
{
    public async Task<Result<ProductVariantDto>> Handle(GetProductVariantQuery request,
        CancellationToken cancellationToken)
    {
        var productVariant =
            await productVariantRepository.GetByIdAsync(request.ProductId, request.Id, cancellationToken);

        if (productVariant is null)
            return Result<ProductVariantDto>.Failure($"Không tìm thấy biến thể với Id '{request.Id}'.");
        var dto = MapToDto(productVariant);
        return Result<ProductVariantDto>.Success(dto);
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
            UpdatedAt = productVariant.UpdatedAt
        };
    }
}