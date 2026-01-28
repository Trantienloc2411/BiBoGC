using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetCategories;

public class GetCategoriesQuery : IRequest<PagedResult<CategoryDto>>
{
    public bool IncludeInactive { get; set; } = false;
    public Guid? ParentCategoryId { get; init; }
}