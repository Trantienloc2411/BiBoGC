namespace InventoryManagement.Domain.ValueObjects;
using Shared.Domain.Common;
public class Money : ValueObject
{
    public decimal Value { get; }

    public Money(decimal value)
    {
        if(value < 0) throw new ArgumentException("Value cannot be negative.", nameof(value));
        Value = value;
    }
    
    
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public static Money operator + (Money left, Money right) => new(left.Value + right.Value);
    public static Money operator -(Money left, Money right) => new(left.Value - right.Value);
    
}