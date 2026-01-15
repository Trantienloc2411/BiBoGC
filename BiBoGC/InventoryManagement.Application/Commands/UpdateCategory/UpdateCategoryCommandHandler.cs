using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using MediatR;
using Shared.Application.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryManagement.Application.Commands.UpdateCategory
{
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
    {
        private readonly ICategoryRepository? _categoryRepository;
        public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category is null)
            {
                return Result<CategoryDto>.Failure("Danh mục không tồn tại");
            }
            else 
            {
                category.UpdateInfo(request.Name, request.Description);
                category.SetParentCategory(request.ParentCategoryId);
                category.SetDisplayOrder(request.DisplayOrder);
                if (request.IsActive)
                {
                    category.Activate();
                }
                else
                {
                    category.Deactivate();
                }
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
}
