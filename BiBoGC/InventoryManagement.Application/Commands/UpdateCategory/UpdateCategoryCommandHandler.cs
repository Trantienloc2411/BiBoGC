using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    private readonly ICategoryRepository? _categoryRepository;

    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        private readonly ICategoryRepository? _categoryRepository;
        public UpdateCategoryCommandHandler (ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            return Result<CategoryDto>.Failure("Danh mục không tồn tại");
        }
        else
        {
            category.UpdateInfo(request.Name, request.Description);
            category.SetParentCategory(request.ParentCategoryId);
            category.SetDisplayOrder(request.DisplayOrder);
            if (request.IsActive)
                category.Activate();
            else
                category.Deactivate();
            await _categoryRepository.UpdateAsync(category);

            return Result<CategoryDto>.Success(new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive
            });
        }
    }
}