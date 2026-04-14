using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetCategoryTree;

public class GetCategoryTreeQueryHandler : IRequestHandler<GetCategoryTreeQuery, Result<IEnumerable<CategoryDto>>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryTreeQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<IEnumerable<CategoryDto>>> Handle(
        GetCategoryTreeQuery request,
        CancellationToken cancellationToken)
    {
        // 1 query – toàn bộ category phẳng
        var allCategories = await _categoryRepository.GetAllAsync(request.IncludeInactive, cancellationToken);

        // 1 query – số sản phẩm theo category (GROUP BY)
        var productCounts = await _categoryRepository.GetProductCountsByCategoryAsync(cancellationToken);

        // Build map id → CategoryDto, SubCategories dùng List<> để append sau
        var dtoMap = allCategories.ToDictionary(
            c => c.Id,
            c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description ?? string.Empty,
                ParentCategoryId = c.ParentCategoryId,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                ProductCount = productCounts.GetValueOrDefault(c.Id, 0),
                SubCategories = new List<CategoryDto>()
            });

        // Điền ParentCategoryName
        foreach (var dto in dtoMap.Values)
        {
            if (dto.ParentCategoryId.HasValue &&
                dtoMap.TryGetValue(dto.ParentCategoryId.Value, out var parent))
                dto.ParentCategoryName = parent.Name;
        }

        // Wire up parent → children
        foreach (var dto in dtoMap.Values)
        {
            if (dto.ParentCategoryId.HasValue &&
                dtoMap.TryGetValue(dto.ParentCategoryId.Value, out var parentDto))
                ((List<CategoryDto>)parentDto.SubCategories!).Add(dto);
        }

        // Sort children ở mỗi node theo DisplayOrder rồi Name
        foreach (var dto in dtoMap.Values)
        {
            ((List<CategoryDto>)dto.SubCategories!)
                .Sort((a, b) => a.DisplayOrder != b.DisplayOrder
                    ? a.DisplayOrder.CompareTo(b.DisplayOrder)
                    : string.Compare(a.Name, b.Name, StringComparison.CurrentCulture));
        }

        IEnumerable<CategoryDto> result;

        if (request.RootId.HasValue)
        {
            if (!dtoMap.TryGetValue(request.RootId.Value, out var rootNode))
                return Result<IEnumerable<CategoryDto>>.Failure("Danh mục không tồn tại.");

            result = [rootNode];
        }
        else
        {
            result = dtoMap.Values
                .Where(d => d.ParentCategoryId == null)
                .OrderBy(d => d.DisplayOrder)
                .ThenBy(d => d.Name);
        }

        return Result<IEnumerable<CategoryDto>>.Success(result);
    }
}
