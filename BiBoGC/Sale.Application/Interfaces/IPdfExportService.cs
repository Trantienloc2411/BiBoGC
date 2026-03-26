using Sale.Application.DTOs;

namespace Sale.Application.Interfaces;

public interface IPdfExportService
{
    byte[] GenerateInvoicePdf(InvoiceDto invoice);
}