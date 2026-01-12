using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.AddBatch;

/// <summary>
/// Command to add a new batch to a product
/// </summary>
/// <remarks>
/// Adds a new batch with quantity to an existing product.
/// Batch numbers must be unique within a product.
/// Expiration date must be after manufacturing date.
/// Used for inventory receiving/stock-in operations.
/// </remarks>
public record AddBatchCommand : IRequest<Result<ProductBatchDto>>
{
    /// <summary>
    /// Product ID to add batch to (required)
    /// </summary>
    public Guid ProductId { get; init; }

    /// <summary>
    /// Batch number/code (required, unique per product, max 50 characters)
    /// </summary>
    /// <example>BATCH-2024-001</example>
    public string BatchNumber { get; init; } = string.Empty;

    /// <summary>
    /// Quantity to add (required, must be > 0)
    /// </summary>
    /// <example>100</example>
    public int Quantity { get; init; }

    /// <summary>
    /// Manufacturing date of the batch (required)
    /// </summary>
    /// <example>2024-01-15</example>
    public DateTime ManufacturingDate { get; init; }

    /// <summary>
    /// Expiration date of the batch (required, must be after manufacturing date)
    /// </summary>
    /// <example>2025-01-15</example>
    public DateTime ExpirationDate { get; init; }

    /// <summary>
    /// Cost price per unit (optional, default 0)
    /// </summary>
    /// <example>10000</example>
    public decimal CostPrice { get; init; } = 0;
}
