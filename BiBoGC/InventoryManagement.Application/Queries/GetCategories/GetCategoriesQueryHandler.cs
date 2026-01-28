using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetCategories;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, PagedResult<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<PagedResult<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Category> categories;

        if (request.ParentCategoryId.HasValue)
            categories =
                await _categoryRepository.GetSubCategoriesAsync(request.ParentCategoryId.Value, cancellationToken);
        else
            categories = await _categoryRepository.GetRootCategoriesAsync(cancellationToken);

        if (!request.IncludeInactive) categories = categories.Where(c => c.IsActive);

        var categoryDtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ParentCategoryId = c.ParentCategoryId,
                ParentCategoryName = c.ParentCategory?.Name,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                ProductCount = c.Items.Count,
                SubCategories = null // For simplicity, not including sub-categories in this example
            })
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToList();

        return new PagedResult<CategoryDto>
        {
            Items = categoryDtos,
            TotalCount = categoryDtos.Count,
            PageSize = categoryDtos.Count,
            Page = 1
        };
    }
}