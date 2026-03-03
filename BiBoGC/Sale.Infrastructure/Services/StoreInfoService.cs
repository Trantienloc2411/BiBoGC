using Microsoft.Extensions.Configuration;
using Sale.Application.Interfaces;
using Sale.Domain.ValueObjects;

namespace Sale.Infrastructure.Services;

public class StoreInfoService : IStoreInfoService
{
    private readonly IConfiguration _configuration;

    public StoreInfoService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<StoreInfo> GetCurrentAsync(CancellationToken ct = default)
    {
        var storeSection = _configuration.GetSection("StoreInfo");

        var storeInfo = new StoreInfo(
            name: storeSection["Name"] ?? "Cửa hàng tạp hóa BiBo",
            address: storeSection["Address"] ?? "123 Đường ABC, Quận XYZ, TP.HCM",
            phone: storeSection["Phone"] ?? "0901234567",
            taxNumber: storeSection["TaxCode"]
        );

        return Task.FromResult(storeInfo);
    }
}