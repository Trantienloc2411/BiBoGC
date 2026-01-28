using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Queries.GetBatch;

public class GetBatchQueryHandler : IRequestHandler<GetBatchQuery, Result<ProductBatchDto>>
{
    private readonly IProductBatchRepository _productBatchRepository;

    public GetBatchQueryHandler(IProductBatchRepository productBatchRepository)
    {
        _productBatchRepository = productBatchRepository;
    }

    public async Task<Result<ProductBatchDto>> Handle(GetBatchQuery request, CancellationToken cancellationToken)
    {
        var batch = await _productBatchRepository.GetByIdAsync(request.Id, cancellationToken);

        if (batch is null) return Result<ProductBatchDto>.Failure($"Không tìm thấy lô hàng với ID '{request.Id}'.");

        var dto = ProductBatchDto.FromEntity(batch);

        return Result<ProductBatchDto>.Success(dto);
    }
}