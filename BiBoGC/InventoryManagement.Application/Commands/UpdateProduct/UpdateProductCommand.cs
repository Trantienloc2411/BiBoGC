using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.UpdateProduct;

/// <summary>
/// Command to update an existing product
/// </summary>
/// <remarks>
/// Updates product information. Only provided fields will be updated.
/// SKU cannot be changed after creation.
/// </remarks>
public record UpdateProductCommand : IRequest<Result<ProductDto>>
{
    /// <summary>
    /// Product ID (required)
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// New product name (optional, max 200 characters)
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// New product price (optional, must be >= 0)
    /// </summary>
    public decimal? Price { get; init; }

    /// <summary>
    /// New product description (optional, max 1000 characters)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// New product status (optional): 1=Active, 2=Inactive, 3=Discontinued, 4=OutOfStock
    /// </summary>
    public int? Status { get; init; }
}