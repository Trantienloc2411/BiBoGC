using Shared.Domain.Common;

namespace Sale.Domain.Domain;

/// <summary>
/// Sequence cho việc tạo số hóa đơn
/// </summary>
public class InvoiceNumberSequence : BaseEntity
{
    private InvoiceNumberSequence()
    {
    }

    public InvoiceNumberSequence(string yearMonth)
    {
        if (string.IsNullOrWhiteSpace(yearMonth) || yearMonth.Length != 6)
            throw new ArgumentException("YearMonth phải có format YYYYMM.", nameof(yearMonth));

        YearMonth = yearMonth;
        CurrentSequence = 0;
    }

    /// <summary>
    /// Năm tháng format YYYYMM (e.g., "202602")
    /// </summary>
    public string YearMonth { get; private set; } = null!;

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