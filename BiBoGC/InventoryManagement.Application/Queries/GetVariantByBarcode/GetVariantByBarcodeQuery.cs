using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetVariantByBarcode;

public record GetVariantByBarcodeQuery(string Barcode) : IRequest<Result<ProductVariantDto>>;