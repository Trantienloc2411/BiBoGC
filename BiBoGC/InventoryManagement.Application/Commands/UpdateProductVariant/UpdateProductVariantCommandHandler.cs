using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.UpdateProductVariant;

public class UpdateProductVariantCommandHandler(IProductVariantRepository productVariantRepository) :
    IRequestHandler<UpdateProductVariantCommand, Result<ProductVariantDto>>
{
    public async Task<Result<ProductVariantDto>> Handle(UpdateProductVariantCommand request,
        CancellationToken cancellationToken)
    {
        var productVariant =
            await productVariantRepository.GetByIdAsync(request.ProductId, request.ProductVariantId, cancellationToken);

        if (productVariant is null)
            return Result<ProductVariantDto>.Failure("Biến thể của sản phẩm không tồn tại, hoặc đã xoá!");

        productVariant.UpdateBarcode(request.Barcode);
        productVariant.UpdatePrice(request.SalePrice, request.CostPrice);
        productVariant.UpdateVariantInfo(request.VariantName, request.Unit, request.QuantityBaseUnit);

        await productVariantRepository.UpdateAsync(productVariant, cancellationToken);

        var dtos = MapToDto(productVariant);
        return Result<ProductVariantDto>.Success(dtos);
    }

    private static ProductVariantDto MapToDto(Domain.Entities.ProductVariant productVariant)
    {
        return new ProductVariantDto
        {
            Id = productVariant.Id,
            SkuUnique = productVariant.SkuUnique,
            VariantName = productVariant.VariantName,
            Unit = productVariant.Unit,
            QuantityBaseUnit = productVariant.QuantityBaseUnit,
            SalePrice = productVariant.SalePrice,
            CostPrice = productVariant.CostPrice,
            Barcode = productVariant.Barcode,
            DisplayOrder = productVariant.DisplayOrder,
            UpdatedAt = DateTime.UtcNow
        };
    }
}