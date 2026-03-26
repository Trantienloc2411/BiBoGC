using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetCategory;

public class GetCategoryQueryHandler : IRequestHandler<GetCategoryQuery, Result<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryDto>> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id);
        if (category is null)
        {
            return Result<CategoryDto>.Failure("Danh mục không thể tìm thấy");
        }
        else
        {
            var subCategoriesListFetch = await _categoryRepository.GetSubCategoriesAsync(request.Id, cancellationToken);
            var mappingSubCategoryDto = subCategoriesListFetch.Select(subCat => new CategoryDto
            {
                Id = subCat.Id,
                Name = subCat.Name,
                Description = subCat.Description,
                ParentCategoryId = subCat.ParentCategoryId,
                ParentCategoryName = subCat.ParentCategory?.Name,
                DisplayOrder = subCat.DisplayOrder,
                IsActive = subCat.IsActive,
                ProductCount = subCat.Items.Count
            });
            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive,
                ProductCount = category.Items.Count,
                SubCategories = mappingSubCategoryDto.Select(subCat => new CategoryDto
                {
                    Id = subCat.Id,
                    Name = subCat.Name,
                    Description = subCat.Description,
                    ParentCategoryId = subCat.ParentCategoryId,
                    ParentCategoryName = subCat.ParentCategoryName,
                    DisplayOrder = subCat.DisplayOrder,
                    IsActive = subCat.IsActive,
                    ProductCount = subCat.ProductCount
                }).ToList()
            };
            return Result<CategoryDto>.Success(categoryDto);
        }
    }
}