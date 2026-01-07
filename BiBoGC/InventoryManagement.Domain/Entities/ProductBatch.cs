using Shared.Domain.Common;

namespace InventoryManagement.Domain.Entities;

public class ProductBatch : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string BatchNumber { get; private set; }
    public int Quantity { get; private set; }
    public DateTime ManufacturingDate { get; private set; }
    public DateTime ExpirationDate { get; private set; }
    public decimal CostPrice { get; private set; }
    
    // Navigation property
    public Product Product { get; private set; } = null!;

    private ProductBatch() { } // For EF Core

    public ProductBatch(
        Guid productId, 
        string batchNumber, 
        int quantity, 
        DateTime manufacturingDate, 
        DateTime expirationDate,
        decimal costPrice = 0)
    {
        if (quantity <= 0) 
            throw new ArgumentException("Số lượng phải lớn hơn 0.");
        if (expirationDate <= manufacturingDate) 
            throw new ArgumentException("Ngày hết hạn phải sau ngày sản xuất.");
        if (string.IsNullOrWhiteSpace(batchNumber))
            throw new ArgumentException("Số lô không được để trống.");

        ProductId = productId;
        BatchNumber = batchNumber.Trim().ToUpper();
        Quantity = quantity;
        ManufacturingDate = manufacturingDate;
        ExpirationDate = expirationDate;
        CostPrice = costPrice;
    }

    public void DecreaseQuantity(int amount)
    {
        if (amount <= 0) 
            throw new ArgumentException("Số lượng giảm phải lớn hơn 0.");
        if (Quantity < amount) 
            throw new InvalidOperationException($"Không đủ hàng trong lô {BatchNumber}. Chỉ còn {Quantity}.");
        
        Quantity -= amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncreaseQuantity(int amount)
    {
        if (amount <= 0) 
            throw new ArgumentException("Số lượng tăng phải lớn hơn 0.");
        
        Quantity += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsExpired(DateTime? checkDate = null)
    {
        var dateToCheck = checkDate ?? DateTime.UtcNow;
        return ExpirationDate <= dateToCheck;
    }

    public int DaysUntilExpiration(DateTime? fromDate = null)
    {
        var dateToCheck = fromDate ?? DateTime.UtcNow;
        return (ExpirationDate.Date - dateToCheck.Date).Days;
    }

    public bool IsExpiringSoon(int warningDays = 30)
    {
        return DaysUntilExpiration() <= warningDays && !IsExpired();
    }
}