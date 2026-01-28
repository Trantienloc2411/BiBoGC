using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using MediatR;

namespace InventoryManagement.Application.Queries.GetBatches;

public class GetBatchesQueryHandler : IRequestHandler<GetBatchesQuery, PaginatedResult<ProductBatchDto>>
{
    private readonly IProductBatchRepository _productBatchRepository;

    public GetBatchesQueryHandler(IProductBatchRepository productBatchRepository)
    {
        _productBatchRepository = productBatchRepository;
    }

    public async Task<PaginatedResult<ProductBatchDto>> Handle(GetBatchesQuery request,
        CancellationToken cancellationToken)
    {
        var (batches, totalCount) = await _productBatchRepository.GetByProductIdAsync(
            request.ProductId,
            request.PageNumber,
            request.PageSize,
            request.IncludeExpired,
            request.SortBy,
            request.SortDescending,
            cancellationToken
        );

        var dtos = batches.Select(ProductBatchDto.FromEntity);

        return new PaginatedResult<ProductBatchDto>
        {
            Items = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}