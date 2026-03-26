using Shared.Domain.Common;

namespace Finance.Domain.Entities;

public class TaxConfiguration : BaseEntity
{
    private TaxConfiguration()
    {
    }

    public string Name { get; private set; } = null!;

    /// <summary>Thuế suất: 0.0000 – 1.0000 (vd: 0.1000 = 10%)</summary>
    public decimal Rate { get; private set; }

    public bool IsEnabled { get; private set; }

    public DateTime EffectiveFrom { get; private set; }

    public static TaxConfiguration Create(string name, decimal rate, bool isEnabled)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên cấu hình thuế không được để trống.", nameof(name));
        if (rate < 0 || rate > 1)
            throw new ArgumentException("Thuế suất phải nằm trong khoảng 0 – 1.", nameof(rate));

        return new TaxConfiguration
        {
            Name = name.Trim(),
            Rate = rate,
            IsEnabled = isEnabled,
            EffectiveFrom = DateTime.UtcNow
        };
    }

    public void Update(decimal rate, bool isEnabled)
    {
        if (rate < 0 || rate > 1)
            throw new ArgumentException("Thuế suất phải nằm trong khoảng 0 – 1.", nameof(rate));

        Rate = rate;
        IsEnabled = isEnabled;
        EffectiveFrom = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}