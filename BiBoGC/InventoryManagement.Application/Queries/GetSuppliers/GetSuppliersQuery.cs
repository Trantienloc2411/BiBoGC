using InventoryManagement.Application.DTOs;
using MediatR;

namespace InventoryManagement.Application.Queries.GetSuppliers;

public class GetSuppliersQuery : IRequest<PaginatedResult<SuppliersDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }

    public bool? IsActive { get; init; } = true;
    public string? SortBy { get; init; } = "Name";
    public bool SortDescending { get; init; } = false;
}