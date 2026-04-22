namespace PricingQuotationAndTerms.Contracts.DTOs;

/// <summary>
/// Shared pricing result consumed by other modules via IPricingApi.
/// </summary>
public class PricingResultDto
{
    public decimal BasePremium   { get; set; }
    public decimal TotalPremium  { get; set; }
    public string  LoadingsJson  { get; set; } = string.Empty;
    public string  DiscountsJson { get; set; } = string.Empty;
    public string  TaxesJson     { get; set; } = string.Empty;
    public string  PricingNotes  { get; set; } = string.Empty;
}
