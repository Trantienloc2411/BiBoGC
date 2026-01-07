using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.DTOs;

public class ProductBatchDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime ManufacturingDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public decimal CostPrice { get; set; }
    public bool IsExpired { get; set; }
    public int DaysUntilExpiration { get; set; }
    public bool IsExpiringSoon { get; set; }
    public DateTime CreatedAt { get; set; }

    public static ProductBatchDto FromEntity(ProductBatch batch)
    {
        return new ProductBatchDto
        {
            Id = (Guid)batch.Id,
            ProductId = batch.ProductId,
            BatchNumber = batch.BatchNumber,
            Quantity = batch.Quantity,
            ManufacturingDate = batch.ManufacturingDate,
            ExpirationDate = batch.ExpirationDate,
            CostPrice = batch.CostPrice,
            IsExpired = batch.IsExpired(),
            DaysUntilExpiration = batch.DaysUntilExpiration(),
            IsExpiringSoon = batch.IsExpiringSoon(),
            CreatedAt = (DateTime)batch.CreatedAt
        };
    }
}