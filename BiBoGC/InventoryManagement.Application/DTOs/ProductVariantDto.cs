using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Application.DTOs;

public class ProductVariantDto
{
    public Guid Id { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public Guid ProductId { get; set; }

    public string? ProductName { get; set; }

    public string? VariantName { get; set; }

    public Units Unit { get; set; }
    public string UnitName { get; set; } = string.Empty;

    public int QuantityBaseUnit { get; set; }

    public decimal SalePrice { get; set; }

    public decimal? CostPrice { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public static ProductVariantDto FromEntity(ProductVariant productVariant)
    {
        return new ProductVariantDto
        {
            Id = productVariant.Id,
            Sku = productVariant.SkuUnique.Value,
            Barcode = productVariant.Barcode,
            ProductId = productVariant.ProductId,
            VariantName = productVariant.VariantName,
            Unit = productVariant.Unit,
            UnitName = productVariant.Unit.ToString(),
            QuantityBaseUnit = productVariant.QuantityBaseUnit,
            SalePrice = productVariant.SalePrice.Value,
            CostPrice = productVariant.CostPrice?.Value,
            DisplayOrder = productVariant.DisplayOrder,
            CreatedAt = productVariant.CreatedAt
        };
    }
}