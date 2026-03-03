namespace Sale.Application.Interfaces;

public interface IOrderNumberGenerator
{
    /// <summary>
    /// Tạo số đơn hàng unique format: SO-YYYYMMDD-XXXX
    /// </summary>
    Task<string> GenerateNextAsync(CancellationToken ct = default);
}