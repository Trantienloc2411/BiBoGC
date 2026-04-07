using InventoryManagement.Application.DTOs;
using InventoryManagement.Domain.Enums;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.CreateProduct;

/// <summary>
/// Command to create a new product
/// </summary>
/// <remarks>
/// This command creates a new product in the inventory system.
/// Required fields: Name, Sku, Price
/// Optional fields: Description, RequiresBatchTracking (default: false)
/// 
/// SKU must be unique across all products.
/// Price must be greater than or equal to 0.
/// </remarks>
public record CreateProductCommand : IRequest<Result<ProductDto>>
{
    /// <summary>
    /// Product name (required, max 200 characters)
    /// </summary>
    /// <example>Coca Cola 330ml</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Stock Keeping Unit - unique identifier for the product (required, max 20 characters)
    /// </summary>
    /// <example>SKU-COCA-330</example>
    public string Sku { get; init; } = string.Empty;

    /// <summary>
    /// Product price (required, must be >= 0)
    /// </summary>
    /// <example>15000</example>
    public decimal Price { get; init; }

    /// <summary>
    /// Product description (optional, max 1000 characters)
    /// </summary>
    /// <example>Nước ngọt có gas Coca Cola lon 330ml</example>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Whether this product requires batch tracking for expiry dates (default: false)
    /// </summary>
    /// <example>true</example>
    public bool RequiresBatchTracking { get; init; } = false;

    public Units BaseUnits { get; init; }
    public int LowStockThreshold { get; init; }
    public Guid? CategoryId { get; init; }
}