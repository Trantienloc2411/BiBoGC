using MediatR;
using Shared.Application.Common;

namespace InventoryManagement.Application.Commands.DeleteCategory;

public class DeleteCategoryCommand : IRequest<Result<Guid>>
{
    public Guid Id { get; set; }
}