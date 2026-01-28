using InventoryManagement.Application.DTOs;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.ValueObjects;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.UpdateProductVariant;

//This command updates a product variant in the inventory system.
// This will allow user update only these fields: VariantName, CostPrice, SalePrice, DisplayOrder and QuantityBaseUnit
public record UpdateProductVariantCommand : IRequest<Result<ProductVariantDto>>
{
    public Guid ProductVariantId { get; init; }
    public Guid ProductId { get; init; }
    public string VariantName { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public int QuantityBaseUnit { get; init; }
    public Money CostPrice { get; init; }
    public Money SalePrice { get; init; }
    public string? Barcode { get; init; } = string.Empty;
    public Units Unit { get; init; }
}