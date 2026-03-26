using InventoryManagement.Application.DTOs;
using MediatR;

namespace InventoryManagement.Application.Queries.GetExpiredBatchProducts;

public record GetExpiredBatchProductsQuery : IRequest<IEnumerable<ProductDto>>;
