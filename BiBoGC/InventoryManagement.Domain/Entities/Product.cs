using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Events.BatchEvents;
using InventoryManagement.Domain.Events.ProductEvents;
using InventoryManagement.Domain.ValueObjects;
using Shared.Domain.Common;

namespace InventoryManagement.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; }
    public Sku Sku { get; private set; }
    public Money Price { get; private set; }
    public string Description { get; private set; }
    public ProductStatuses Status { get; private set; }
    public bool RequiresBatchTracking { get; private set; }

    private readonly List<ProductBatch> _batches = new();
    public IReadOnlyCollection<ProductBatch> Batches => _batches.AsReadOnly();
    
    private Product() { }
    public Product(string name, Sku sku, Money price, string description, ProductStatuses status, bool requiresBatchTracking)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Sku = sku ?? throw new ArgumentNullException(nameof(sku));
        Price = price ?? throw new ArgumentNullException(nameof(price));
        Description = description ?? string.Empty;
        Status = ProductStatuses.Active;
        RequiresBatchTracking = requiresBatchTracking;
        
        RaiseDomainEvent(new ProductCreatedEvent(Id, name, sku.Value));
    }

    public int GetTotalStock(DateTime? asOfDate = null)
    {
        var checkdate = asOfDate ?? DateTime.UtcNow;
        return _batches.Where(b => !b.IsExpired(checkdate))
            .Sum(b => b.Quantity);
    }

    public int GetAvailableStock(DateTime? asOfDate = null)
    {
        var checkdate = asOfDate ?? DateTime.UtcNow;
        return _batches.Where(b => !b.IsExpired(checkdate) && b.Quantity > 0)
            .Sum(b => b.Quantity);
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
        {
            throw new InvalidOperationException($"Lô hàng '{batchNumber} đã tồn tại cho sản phẩm '{Sku.Value}'");
        }
        var newBatch = new ProductBatch(Id, batchNumber, quantity, manufacturingDate, expiryDate, costPrice);
        _batches.Add(newBatch);
        UpdatedAt = DateTime.UtcNow;
        
        RaiseDomainEvent(new BatchAddedEvent(Id,  newBatch.Id, quantity));
        
        return newBatch;
    }

    public void UpdatePrice(Money newPrice)
    {
        if(newPrice.Value < 0) throw new ArgumentException("Price cannot be negative.", nameof(newPrice));
        var oldPrice = Price;
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
        
        RaiseDomainEvent(new ProductPriceChangedEvent(Id, oldPrice.Value, newPrice.Value));
    }

    public void Discontinue()
    {
        if(Status == ProductStatuses.Discontinued) throw new InvalidOperationException("Product is already discontinued.");

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