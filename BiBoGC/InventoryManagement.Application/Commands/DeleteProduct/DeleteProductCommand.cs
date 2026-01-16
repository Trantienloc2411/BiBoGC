using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.DeleteProduct;

/// <summary>
/// Command to soft-delete a product
/// </summary>
/// <remarks>
/// Performs a soft delete on the product (sets IsDeleted flag).
/// The product will no longer appear in queries but data is preserved.
/// </remarks>
public record DeleteProductCommand : IRequest<Result>
{
    /// <summary>
    /// Product ID to delete (required)
    /// </summary>
    public Guid Id { get; init; }
}
