using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (await _categoryRepository.NameExistsAsync(request.Name, null, cancellationToken))
            return Result<CategoryDto>.Failure($"Category with name '{request.Name}' already exists.");

        if (request.ParentCategoryId.HasValue)
        {
            var parentCategory =
                await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);
            if (parentCategory is not null)
                return Result<CategoryDto>.Failure(
                    $"Parent category with ID '{request.ParentCategoryId}' does not exist.");
        }

        var category = new Category(request.Name, request.Description, request.ParentCategoryId);
        if (request.DisplayOrder > 0) category.SetDisplayOrder(request.DisplayOrder);

        var createdCategory = await _categoryRepository.AddAsync(category, cancellationToken);

        var dto = new CategoryDto
        {
            Id = createdCategory.Id,
            Name = createdCategory.Name,
            Description = createdCategory.Description,
            ParentCategoryId = createdCategory.ParentCategoryId,
            DisplayOrder = createdCategory.DisplayOrder,
            IsActive = createdCategory.IsActive
        };

        return Result<CategoryDto>.Success(dto);
    }
}