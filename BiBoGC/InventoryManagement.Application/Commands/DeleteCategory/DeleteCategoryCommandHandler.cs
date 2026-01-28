using InventoryManagement.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result<Guid>>
{
    private readonly ICategoryRepository _categoryRepository;

    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<Guid>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id);
        if (category is null) return Result<Guid>.Failure($"Danh mục có mã {request.Id} không tồn tại");

            // adding logic for cascading delete

            var subCategories = await _categoryRepository.GetSubCategoriesAsync(request.Id);

            if(subCategories.Count() > 0)
            {
                return Result<Guid>.Failure($"Danh mục này có danh mục con, hãy xóa danh mục con trước.");
            }


            await _categoryRepository.DeleteAsync(category, cancellationToken);


            
            return Result<Guid>.Success(request.Id);
        }
    }
}