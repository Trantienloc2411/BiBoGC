using Shared.Domain.Common;

namespace InventoryManagement.Domain.Entities;

public class Supplier : BaseEntity
{
    private readonly List<StockTransaction> _transactions = new();

    private Supplier()
    {
    }

    public Supplier(
        string name,
        string? contactPerson = null,
        string? phoneNumber = null,
        string? address = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên nhà cung cấp không được để trống.", nameof(name));

        Name = name.Trim();
        ContactPerson = contactPerson?.Trim();
        PhoneNumber = phoneNumber?.Trim();
        Address = address?.Trim();
    }

    public string Name { get; set; } = null!;
    public string? ContactPerson { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public IReadOnlyCollection<StockTransaction> Transactions => _transactions.AsReadOnly();

    public void UpdateInfo(
        string name,
        string? contactPerson,
        string? phoneNumber,
        string? address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên nhà cung cấp không được để trống.", nameof(name));

        Name = name.Trim();
        ContactPerson = contactPerson?.Trim();
        PhoneNumber = phoneNumber?.Trim();
        Address = address?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}