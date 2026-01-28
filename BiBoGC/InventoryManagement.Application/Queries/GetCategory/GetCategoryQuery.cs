using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetCategory;

public class GetCategoryQuery : IRequest<Result<CategoryDto>>
{
    public Guid Id { get; set; }
}