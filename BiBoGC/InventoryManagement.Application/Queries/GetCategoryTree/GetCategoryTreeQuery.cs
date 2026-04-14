using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetCategoryTree;

/// <summary>
/// Trả về toàn bộ cây danh mục dạng nested (folder-tree).
/// - RootId = null  → trả về tất cả cây từ các root node.
/// - RootId = guid  → trả về cây bắt đầu từ node đó (node + toàn bộ con cháu).
/// </summary>
public class GetCategoryTreeQuery : IRequest<Result<IEnumerable<CategoryDto>>>
{
    public Guid? RootId { get; set; }
    public bool IncludeInactive { get; set; } = false;
}
