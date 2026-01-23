namespace InventoryManagement.Domain.ValueObjects;
using Shared.Domain.Common;
public class Sku : ValueObject
{
    public string Value { get; }

    public Sku(string value)
    {
        if(string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
        if(value.Length > 50) throw new ArgumentException("Value cannot be longer than 50 characters.", nameof(value));

        Value = value.Trim().ToUpper();
    }
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public static implicit operator string(Sku sku) => sku.Value;
    public static implicit operator Sku(string value) => new(value);
}