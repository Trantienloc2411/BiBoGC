namespace Finance.Application.DTOs;

public class TaxConfigDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Rate { get; set; }
    public decimal RatePercent => Rate * 100;
    public bool IsEnabled { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TaxConfigsDto
{
    public TaxConfigDto Vat { get; set; } = null!;
    public TaxConfigDto Pit { get; set; } = null!;
}