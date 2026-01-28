using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Helper;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.ValueObjects;
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
        
        var doesVariantExisted = await productVariantRepository.DoesVariantExistAsync(request.ProductId,  request.ProductVariantId,request.QuantityBaseUnit, request.Unit, cancellationToken);
        if (doesVariantExisted)
            return Result<ProductVariantDto>.Failure("Biến thể đã tồn tại trong hệ thống, không thể đè cập nhật mới!");
        
        var newSkuUnique = AutoGenerateSkuUnique.GenerateSkuUnique(productVariant.Product.SkuGeneral, request.QuantityBaseUnit, request.Unit);
        
        productVariant.UpdateBarcode(request.Barcode);
        productVariant.UpdatePrice(new Money(request.SalePrice), new Money(request.CostPrice));
        productVariant.UpdateVariantInfo(request.VariantName, request.Unit, request.QuantityBaseUnit, newSkuUnique);
        productVariant.SetDisplayOrder(request.DisplayOrder);   
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