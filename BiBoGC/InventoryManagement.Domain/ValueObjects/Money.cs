namespace InventoryManagement.Domain.ValueObjects;

using Shared.Domain.Common;

public class Money : ValueObject
{
    public Money(decimal value)
    {
        if (value < 0) throw new ArgumentException("Value cannot be negative.", nameof(value));
        Value = value;
    }

    public decimal Value { get; }


    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static Money operator +(Money left, Money right)
    {
        return new Money(left.Value + right.Value);
    }

    public static Money operator -(Money left, Money right)
    {
        return new Money(left.Value - right.Value);
    }
}