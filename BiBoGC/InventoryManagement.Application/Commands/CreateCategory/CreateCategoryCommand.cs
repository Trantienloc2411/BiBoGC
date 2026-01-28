using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.CreateCategory;

public class CreateCategoryCommand : IRequest<Result<CategoryDto>>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public Guid? ParentCategoryId { get; set; }
}