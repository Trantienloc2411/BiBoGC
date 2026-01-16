using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryManagement.Application.Queries.GetSupplier
{
    public class GetSupplierQuery : IRequest<Result<SupplierDto>>
    {
        public Guid SupplierId { get; init; }

    }
}
