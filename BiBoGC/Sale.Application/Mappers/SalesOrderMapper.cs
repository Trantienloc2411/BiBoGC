using Sale.Application.DTOs;
using Sale.Domain.Domain;

namespace Sale.Application.Mappers;

public static class SalesOrderMapper
{
    public static SalesOrderDto MapToDto(SalesOrder order)
    {
        return new SalesOrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            OrderDate = order.OrderDate,
            SubTotal = order.SubTotal,
            DiscountAmount = order.DiscountAmount,
            TaxAmount = order.TaxAmount,
            TotalAmount = order.TotalAmount,
            AmountPaid = order.AmountPaid,
            ChangeAmount = order.ChangeAmount,
            PaymentMethod = order.PaymentMethod.ToString(),
            Status = order.Status.ToString(),
            Notes = order.Notes,
            InvoiceId = order.InvoiceId,
            ItemCount = order.Items.Count,
            Items = order.Items.Select(MapItemToDto),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }

    public static SalesOrderItemDto MapItemToDto(SalesOrderItem item)
    {
        return new SalesOrderItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductVariantId = item.ProductVariantId,
            ProductBatchId = item.ProductBatchId,
            ProductName = item.ProductName,
            VariantName = item.VariantName,
            Sku = item.Sku,
            Unit = item.Unit,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            LineTotal = item.LineTotal
        };
    }
}