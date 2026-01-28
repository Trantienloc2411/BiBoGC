namespace InventoryManagement.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool RequiresBatchTracking { get; set; }
    public int TotalStock { get; set; }
    public int AvailableStock { get; set; }
    public int ExpiredStock { get; set; }
    public int ExpiringSoonStock { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public IEnumerable<ProductBatchDto> RecentBatches { get; set; } = new List<ProductBatchDto>();
    public IEnumerable<ProductVariantDto>? Variants { get; set; } = new List<ProductVariantDto>();
}