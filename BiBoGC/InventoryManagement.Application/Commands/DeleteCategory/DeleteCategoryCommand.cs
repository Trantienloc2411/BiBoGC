using MediatR;
using Shared.Application.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryManagement.Application.Commands.DeleteCategory
{
    public class DeleteCategoryCommand : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; }
    }
}
