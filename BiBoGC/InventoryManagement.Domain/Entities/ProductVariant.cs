using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.ValueObjects;
using Shared.Domain.Common;

namespace InventoryManagement.Domain.Entities;

public class ProductVariant : BaseEntity    
{
    //Will auto generate base on format: [SkuGeneral]-[QuantityBaseUnit]
    //e.g COCA-LON-24
    public Sku SkuUnique { get; private set; }
    public string? Barcode { get; private set; }    
    public Guid ProductId { get; private set; }
    public string VariantName { get; private set; }
    public Units  Unit { get; private set; }
    public int QuantityBaseUnit { get; private set; }
    public Money SalePrice { get; private set; }    
    public Money? CostPrice { get; private set; }
    
     public bool IsActive { get; private set; } = true;
     public int DisplayOrder { get; private set; }
     
     public Product? Product { get; set; }  
     private ProductVariant() {}

     public ProductVariant(
         Guid productId,
         string sku,
         string variantName,
         Units unit,
         int quantityBaseUnit,
         Money salePrice,
         string? barcode,
         Money? costPrice,
         int displayOrder = 0
     )
     {
         if(string.IsNullOrWhiteSpace(sku)) 
             throw new  ArgumentNullException("SKU không được để trống!",nameof(sku));
         if(string.IsNullOrWhiteSpace(variantName)) 
             throw new ArgumentException("Tên biến thể không được để trống", nameof(variantName));
         
         if(quantityBaseUnit <= 0) 
             throw new ArgumentException("Số lượng đơn vị cơ sở phải lớn hơn 0.", nameof(quantityBaseUnit));
         
         if(salePrice.Value < 0)
             throw new ArgumentException("Giá bán không được âm.", nameof(salePrice));  
         
         ProductId = productId; 
         SkuUnique = sku;
         VariantName = variantName.Trim();
         Unit = unit;
         QuantityBaseUnit = quantityBaseUnit;
         SalePrice = salePrice;
         CostPrice = costPrice;
         DisplayOrder = displayOrder;
         IsActive = true;
         Barcode = string.IsNullOrEmpty(barcode) ? null : barcode.Trim();
     }

     public void UpdatePrice(Money newSalePrice, Money? newCostPrice)
     {
         if(newSalePrice.Value < 0) 
             throw new ArgumentException("Giá bán không được âm. ", nameof(newSalePrice));
         SalePrice = newSalePrice;


         if (newCostPrice is not null)
         {
             if(newCostPrice.Value < 0)
                 throw new ArgumentException("Giá gốc không được âm . ", nameof(newCostPrice));
             
             CostPrice = newCostPrice;
         }    
         UpdatedAt  = DateTime.UtcNow;
     }

     public void UpdateBarcode(string barcode)
     {
         Barcode = barcode.Trim();
         UpdatedAt  = DateTime.UtcNow;
     }

     public void UpdateVariantInfo(string variantName, Units unit, int quantityBaseUnit)
     {
         if(string.IsNullOrWhiteSpace(variantName)) 
             throw new ArgumentException("Tên biến thể không được để trống", nameof(variantName));
         if(quantityBaseUnit <= 0)
             throw new ArgumentException("Số lượng đơn vị cơ sở phải lớn hơn không.", nameof(quantityBaseUnit));
         
         VariantName = variantName.Trim(); 
         Unit = unit;
         QuantityBaseUnit = quantityBaseUnit;
     }
     
     public void SetDisplayOrder(int displayOrder)
     {
         DisplayOrder = displayOrder;
         UpdatedAt  = DateTime.UtcNow;
     }

     public void Deactivate()
     {
         IsActive = false;
         UpdatedAt  = DateTime.UtcNow;
     }

     public decimal GetPricePerUnit()
     {
         return SalePrice.Value / QuantityBaseUnit;
     }

     public int ConvertToBaseUnit(int variantQuantity)
     {
         return variantQuantity * QuantityBaseUnit;
     }

     public (int fullVariant, int remainder) ConvertFromBaseUnit(int baseUnitQuantity)
     {
         var fullVariants = baseUnitQuantity / QuantityBaseUnit;
         var remainder = baseUnitQuantity % QuantityBaseUnit;
         return (fullVariants, remainder);
     }



}