using Sale.Domain.ValueObjects;

namespace Sale.Application.Interfaces;

public interface IStoreInfoService
{
    /// <summary>
    /// Lấy thông tin cửa hàng hiện tại
    /// </summary>
    Task<StoreInfo> GetCurrentAsync(CancellationToken ct = default);
}