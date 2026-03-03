using Shared.Domain.Common;

namespace Sale.Domain.ValueObjects;

public class StoreInfo : ValueObject
{
    public StoreInfo(string name, string address, string phone, string? taxNumber = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException("Tên cửa hàng không thể để trống. ", nameof(name));
        if (string.IsNullOrEmpty(address))
            throw new ArgumentNullException("Địa chỉ cửa hàng không được để trống. ", nameof(address));
        if (string.IsNullOrEmpty(phone))
            throw new ArgumentNullException("Số điện thoại của cửa hàng không được để trống. ", nameof(phone));

        Name = name.Trim();
        Address = address.Trim();
        Phone = phone.Trim();
        TaxNumber = taxNumber?.Trim();
    }

    public string Name { get; }
    public string Address { get; }
    public string Phone { get; set; }
    public string? TaxNumber { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Address;
        yield return Phone;
        yield return TaxNumber ?? string.Empty;
    }
}