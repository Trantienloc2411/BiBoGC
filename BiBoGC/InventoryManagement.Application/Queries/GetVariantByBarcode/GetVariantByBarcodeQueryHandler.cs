using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetVariantByBarcode;

public class GetVariantByBarcodeQueryHandler : IRequestHandler<GetVariantByBarcodeQuery, Result<ProductVariantDto>>
{
    private readonly IProductVariantRepository _variantRepository;

    public GetVariantByBarcodeQueryHandler(IProductVariantRepository variantRepository)
    {
        _variantRepository = variantRepository;
    }

    public async Task<Result<ProductVariantDto>> Handle(
        GetVariantByBarcodeQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Barcode))
            return Result<ProductVariantDto>.Failure("Barcode không được để trống.");

        var variant = await _variantRepository.GetByBarcodeAsync(request.Barcode.Trim(), cancellationToken);

        if (variant is null)
            return Result<ProductVariantDto>.Failure($"Không tìm thấy sản phẩm với barcode '{request.Barcode}'.");

        return Result<ProductVariantDto>.Success(ProductVariantDto.FromEntity(variant));
    }
}