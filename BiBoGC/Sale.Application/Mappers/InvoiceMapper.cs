using Sale.Application.DTOs;
using Sale.Domain.Domain;

namespace Sale.Application.Mappers;

public static class InvoiceMapper
{
    public static InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            SalesOrderId = invoice.SalesOrderId,
            OrderNumber = invoice.OrderNumber,
            StoreName = invoice.StoreName,
            StoreAddress = invoice.StoreAddress,
            StorePhone = invoice.StorePhone,
            StoreTaxCode = invoice.StoreTaxCode,
            CustomerName = invoice.CustomerName,
            CustomerPhone = invoice.CustomerPhone,
            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            TaxAmount = invoice.TaxAmount,
            GrandTotal = invoice.GrandTotal,
            AmountPaid = invoice.AmountPaid,
            ChangeAmount = invoice.ChangeAmount,
            PaymentMethod = invoice.PaymentMethod,
            Items = invoice.Items.Select(MapItemToDto),
            CreatedAt = invoice.CreatedAt
        };
    }

    public static InvoiceItemDto MapItemToDto(InvoiceItem item)
    {
        return new InvoiceItemDto
        {
            Id = item.Id,
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