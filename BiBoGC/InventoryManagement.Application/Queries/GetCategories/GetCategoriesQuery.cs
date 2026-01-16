using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryManagement.Application.Queries.GetCategories
{
    public class GetCategoriesQuery : IRequest<PagedResult<CategoryDto>>
    {
        public bool IncludeInactive { get; set; } = false;
        public Guid? ParentCategoryId { get; init; } 
    }
}
