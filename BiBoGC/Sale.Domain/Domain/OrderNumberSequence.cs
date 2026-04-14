using Shared.Domain.Common;

namespace Sale.Domain.Domain;

/// <summary>
/// Sequence cho việc tạo số đơn hàng theo ngày
/// </summary>
public class OrderNumberSequence : BaseEntity
{
    private OrderNumberSequence()
    {
    }

    public OrderNumberSequence(string date)
    {
        if (string.IsNullOrWhiteSpace(date) || date.Length != 8)
            throw new ArgumentException("Date phải có format YYYYMMDD.", nameof(date));

        Date = date;
        CurrentSequence = 0;
    }

    /// <summary>
    /// Ngày format YYYYMMDD (e.g., "20260414")
    /// </summary>
    public string Date { get; private set; } = null!;

    /// <summary>
    /// Số sequence hiện tại
    /// </summary>
    public int CurrentSequence { get; private set; }

    /// <summary>
    /// Tăng sequence và trả về giá trị mới
    /// </summary>
    public int Increment()
    {
        CurrentSequence++;
        UpdatedAt = DateTime.UtcNow;
        return CurrentSequence;
    }
}
