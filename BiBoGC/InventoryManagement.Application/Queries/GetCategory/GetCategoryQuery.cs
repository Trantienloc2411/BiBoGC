using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryManagement.Application.Queries.GetCategory
{
    public class GetCategoryQuery : IRequest<Result<CategoryDto>>
    {
        public Guid Id { get; set; }
    }
}
