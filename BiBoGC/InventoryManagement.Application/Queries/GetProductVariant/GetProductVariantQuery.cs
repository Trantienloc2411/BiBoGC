using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetProductVariant;

public record GetProductVariantQuery : IRequest<Result<ProductVariantDto>>
{
    public Guid ProductId { get; init; }
    public Guid Id { get; init; }
}