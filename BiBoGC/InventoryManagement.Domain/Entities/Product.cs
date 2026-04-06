using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Events.BatchEvents;
using InventoryManagement.Domain.Events.ProductEvents;
using InventoryManagement.Domain.Events.ProductVariantEvents;
using InventoryManagement.Domain.ValueObjects;
using Shared.Domain.Common;

namespace InventoryManagement.Domain.Entities;

public class Product : BaseEntity
{
    private readonly List<ProductBatch> _batches = new();
    private readonly List<ProductVariant> _variants = new();

    private Product()
    {
    }

    public Product(
        string name,
        string description,
        ProductStatuses status,
        Sku skuGeneral,
        bool requiresBatchTracking,
        Units baseUnits,
        Money basePrice,
        int? lowStockThreshold,
        Guid? categoryId = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        BasePrice = basePrice ?? throw new ArgumentNullException(nameof(basePrice));
        BaseUnits = baseUnits;
        SkuGeneral = skuGeneral;
        Description = description ?? string.Empty;
        Status = ProductStatuses.Active;
        RequiresBatchTracking = requiresBatchTracking;
        LowStockThreshold = lowStockThreshold;
        CategoryId = categoryId;


        RaiseDomainEvent(new ProductCreatedEvent(Id, name));
    }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public ProductStatuses Status { get; private set; }
    public bool RequiresBatchTracking { get; private set; }
    public Guid? CategoryId { get; private set; }

    public Sku SkuGeneral { get; private set; }

    public Units BaseUnits { get; private set; }

    public int TotalStock { get; private set; }

    /// <summary>
    /// Base price of the product. (Giá nêm yết/ giá gốc bán ra)
    /// - Dùng để: So sánh với giá bán 
    /// </summary>

    public Money BasePrice { get; private set; }

    public Money? AverageCostPrice { get; private set; }

    public int? LowStockThreshold { get; private set; }

    public Category? Category { get; private set; }
    public IReadOnlyCollection<ProductBatch> Batches => _batches.AsReadOnly();
    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();


    // ==== Stock management ==== //


    /// <summary>
    /// Lấy tổng tồn kho từ các lô hàng chưa hết hạn (nếu có batchTracking)
    /// </summary>
    /// <param name="asOfDate"></param>
    /// <returns></returns>
    public int GetStockFromBatches(DateTime? asOfDate = null)
    {
        var checkDate = asOfDate ?? DateTime.UtcNow;
        return _batches.Where(b => !b.IsExpired(checkDate))
            .Sum(b => b.Quantity);
    }


    public void IncreaseStock(int quantityBaseUnit)
    {
        if (quantityBaseUnit <= 0)
            throw new ArgumentOutOfRangeException("Số lượng phải lớn hơn 0", nameof(quantityBaseUnit));

        TotalStock += quantityBaseUnit;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecreaseStock(int quantityBaseUnit)
    {
        if (quantityBaseUnit <= 0)
            throw new ArgumentOutOfRangeException("Số lượng phải lớn hơn 0", nameof(quantityBaseUnit));

        if (TotalStock < quantityBaseUnit)
            throw new InvalidOperationException($"Không đủ số lượng hàng trong kho. Tồn kho hiện tại: {TotalStock}.");

        TotalStock -= quantityBaseUnit;
        if (TotalStock == 0)
            Status = ProductStatuses.OutOfStock;
        UpdatedAt = DateTime.UtcNow;
    }


    public bool IsLowStock()
    {
        if (!LowStockThreshold.HasValue) return false;
        return TotalStock < LowStockThreshold.Value;
    }


    public int GetAvailableStock(DateTime? asOfDate = null)
    {
        var checkdate = asOfDate ?? DateTime.UtcNow;
        return _batches.Where(b => !b.IsExpired(checkdate) && b.Quantity > 0)
            .Sum(b => b.Quantity);
    }

    public void SetCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    public IEnumerable<ProductBatch> GetExpiredBatches(DateTime? asOfDate = null)
    {
        var checkDate = asOfDate ?? DateTime.UtcNow;
        return _batches.Where(b => b.IsExpired(checkDate));
    }

    public IEnumerable<ProductBatch> GetExpiringSoonBatches(int warningDays = 30)
    {
        return _batches.Where(b => b.IsExpiringSoon(warningDays));
    }

    public ProductBatch AddNewBatch(
        string batchNumber,
        int quantity,
        DateTime manufacturingDate,
        DateTime expiryDate,
        decimal costPrice = 0
    )
    {
        if (_batches.Any(b => b.BatchNumber.Equals(batchNumber.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Lô hàng '{batchNumber} đã tồn tại cho sản phẩm.");
        var newBatch = new ProductBatch(Id, batchNumber, quantity, manufacturingDate, expiryDate, costPrice);
        _batches.Add(newBatch);
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new BatchAddedEvent(Id, newBatch.Id, quantity));

        return newBatch;
    }

    public ProductVariant AddProductVariant(
        string sku,
        Guid productId,
        string variantName,
        Units units,
        int quantityBaseUnit,
        Money salePrice,
        int displayOrder,
        string? barcode,
        Money? costPrice
    )
    {
        if (_variants.Any(v => v.SkuUnique.Value.Equals(sku.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Mã SKU '{sku} đã tồn tại trong hệ thống.");

        var newVariant = new ProductVariant(productId, sku, variantName, units, quantityBaseUnit, salePrice, barcode,
            costPrice, displayOrder);
        _variants.Add(newVariant);
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ProductVariantCreatedEvent(Id, newVariant.Id, variantName));
        return newVariant;
    }

    public void UpdatePrice(Money newPrice)
    {
        if (newPrice.Value < 0) throw new ArgumentException("Giá không thể âm. ", nameof(newPrice));
        var oldPrice = BasePrice;
        BasePrice = newPrice;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ProductPriceChangedEvent(Id, oldPrice.Value, newPrice.Value));
    }

    public void Discontinue()
    {
        if (Status == ProductStatuses.Discontinued)
            throw new InvalidOperationException("Product is already discontinued.");

        Status = ProductStatuses.Discontinued;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ProductDiscontinuedEvent(Id, Name));
    }

    public IEnumerable<ProductBatch> GetBatchesForSale(int requestedQuantity)
    {
        var availableBatches = _batches
            .Where(b => !b.IsExpired() && b.Quantity > 0)
            .OrderBy(b => b.ExpirationDate)
            .ThenBy(b => b.ManufacturingDate)
            .ToList();

        var result = new List<ProductBatch>();
        var remainingQuantity = requestedQuantity;

        foreach (var batch in availableBatches)
        {
            if (remainingQuantity <= 0) break;

            result.Add(batch);
            remainingQuantity -= batch.Quantity;
        }

        return result;
    }
}