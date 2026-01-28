using InventoryManagement.Application.Commands.AddProductVariant;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Helper;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.ValueObjects;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.CreateProductVariant;

public class CreateProductVariantCommandHandler(
    IProductVariantRepository productVariantRepository,
    IProductRepository productRepository) :
    IRequestHandler<CreateProductVariantCommand, Result<ProductVariantDto>>
{
    public async Task<Result<ProductVariantDto>>
        Handle(CreateProductVariantCommand request,
            CancellationToken cancellationToken)
    {
        var doesVariantExist =
            await productVariantRepository.DoesVariantExistAsync(request.ProductId,Guid.Empty, request.QuantityBaseUnit,
                request.Unit, cancellationToken);
        if (doesVariantExist)
            return Result<ProductVariantDto>.Failure(
                "Biến thể này đã tồn tại. Hãy thay đổi đơn vị/số lượng trên đơn vị");
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
            return Result<ProductVariantDto>.Failure("Sản phẩm này không tồn tại. Hãy thêm sản phẩm trước!");

        var productVariant = new ProductVariant(
            request.ProductId,
            AutoGenerateSkuUnique.GenerateSkuUnique(product.SkuGeneral, request.QuantityBaseUnit, request.Unit),
            request.VariantName,
            quantityBaseUnit: request.QuantityBaseUnit,
            salePrice: new Money(request.SalePrice),
            unit: request.Unit,
            barcode: request.Barcode,
            costPrice: new Money(request.CostPrice),
            displayOrder: request.DisplayOrder
        );

        await productVariantRepository.AddAsync(productVariant, cancellationToken);

        var dto = MapToDto(productVariant);
        dto.ProductName = product.Name;
        return Result<ProductVariantDto>.Success(dto);
    }

    private static ProductVariantDto MapToDto(ProductVariant productVariant)
    {
        return new ProductVariantDto
        {
            Id = productVariant.Id,
            SkuUnique = productVariant.SkuUnique,
            VariantName = productVariant.VariantName,
            QuantityBaseUnit = productVariant.QuantityBaseUnit,
            SalePrice = productVariant.SalePrice,
            Unit = productVariant.Unit,
            Barcode = productVariant.Barcode,
            CostPrice = productVariant.CostPrice,
            DisplayOrder = productVariant.DisplayOrder,
            ProductId = productVariant.ProductId,
            ProductName = productVariant.Product?.Name ?? string.Empty
        };
    }
}