using QuestPDF.Fluent;
using Sale.Application.DTOs;
using Sale.Application.Interfaces;
using Sale.Infrastructure.Pdf;

namespace Sale.Infrastructure.Services;

public class PdfExportService : IPdfExportService
{
    public byte[] GenerateInvoicePdf(InvoiceDto invoice)
    {
        var document = new InvoicePdfDocument(invoice);
        return document.GeneratePdf();
    }
}