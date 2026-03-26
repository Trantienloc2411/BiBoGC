using InventoryManagement.Application.DTOs;
using MediatR;

namespace InventoryManagement.Application.Queries.GetLowStockProducts;

public record GetLowStockProductsQuery : IRequest<IEnumerable<ProductDto>>;
