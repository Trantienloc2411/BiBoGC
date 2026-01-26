using InventoryManagement.Application.DTOs;
using MediatR;
using Shared.Application.Common;
using System;
using System.Collections.Generic;
using System.Text;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.ValueObjects;

namespace InventoryManagement.Application.Commands.AddProductVariant
{
    public class CreateProductVariantCommand : IRequest<Result<ProductVariantDto>>
    {
        public Guid ProductId { get; init; }
        public string VariantName { get; init; } = string.Empty;
        public string? Barcode { get; init; }
        public int DisplayOrder { get; init; }
        public Units Unit { get; init; }
        public Money SalePrice { get; init; }
        public Money CostPrice { get; init; }
        public int QuantityBaseUnit { get; init; }
    }
}
