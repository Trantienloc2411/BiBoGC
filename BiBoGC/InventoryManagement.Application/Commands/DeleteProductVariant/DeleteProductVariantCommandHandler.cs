using InventoryManagement.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.DeleteProductVariant;

public class DeleteProductVariantCommandHandler(IProductVariantRepository productVariantRepository) :
    IRequestHandler<DeleteProductVariantCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteProductVariantCommand request, CancellationToken cancellationToken)
    {
        var productVariant =
            await productVariantRepository.GetByIdAsync(request.ProductId, request.Id, cancellationToken);

        if (productVariant is null)
            return Result<bool>.Failure("Biến thể này đã được xoá hoặc không tồn tại!");

        await productVariantRepository.DeleteAsync(productVariant, cancellationToken);
        return Result<bool>.Success(true);
    }
}