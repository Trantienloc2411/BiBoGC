namespace Finance.Application.Interfaces;

public record TaxDeclarationData(
    int CurrentYear,
    int MonthFrom,
    int YearFrom,
    int MonthTo,
    int YearTo,
    decimal Revenue,
    decimal GtgtTaxAmount, // Revenue × 1%
    decimal TncnTaxAmount, // Revenue × 0.5%
    int DayExport,
    int MonthExport,
    int YearExport);

public interface ITaxDeclarationExportService
{
    /// <summary>
    /// Fills the 01/CNKD template (.docx) with actual data and returns the filled document bytes.
    /// </summary>
    byte[] GenerateTaxDeclarationDocx(TaxDeclarationData data);
}