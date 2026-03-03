namespace Sale.Application.Interfaces;

public interface IInvoiceNumberGenerator
{
    /// <summary>
    /// Tạo số hóa đơn unique format: INV-YYYYMM-XXXX
    /// </summary>
    Task<string> GenerateNextAsync(CancellationToken ct = default);
}