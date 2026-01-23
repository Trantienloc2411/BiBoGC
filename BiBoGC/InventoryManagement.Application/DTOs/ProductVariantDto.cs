using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.ValueObjects;

namespace InventoryManagement.Application.DTOs;

public class ProductVariantDto
{
    public Guid Id { get; set; }
    
    public Sku SkuUnique { get; set; }
    
    public string? Barcode { get; set; }
    
    public Guid ProductId { get; set; }
    
    public string? ProductName { get; set; }
    
    public string? VariantName { get; set; }
    
    public Units Unit { get; set; }
    
    public int QuantityBaseUnit { get; set; }
    
    public Money SalePrice { get; set; }
    
    public Money? CostPrice { get; set; }
    
    public int DisplayOrder { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
}

