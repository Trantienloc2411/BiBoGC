using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetProductVariant;

public class GetProductVariantQuery : IRequest<Result<ProductVariantDto>>
{
    public Guid Id { get; init; }    
}