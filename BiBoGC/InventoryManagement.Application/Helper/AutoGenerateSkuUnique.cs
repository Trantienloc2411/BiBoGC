using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Application.Helper;

public static class AutoGenerateSkuUnique
{
    public static string GenerateSkuUnique(string skuGeneral, int quantityBaseUnit, Units unit)
    {
        return $"{skuGeneral}-{quantityBaseUnit}-{unit.ToString().ToLowerInvariant()}";
    }
}