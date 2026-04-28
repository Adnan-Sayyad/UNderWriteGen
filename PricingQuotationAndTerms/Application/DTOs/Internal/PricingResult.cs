namespace PricingQuotationAndTerms.Application.DTOs.Internal;

/// <summary>
/// Full breakdown produced by PricingService.
/// This is INTERNAL — never expose this directly to API responses or other modules.
/// QuoteService maps it to JSON fields on the Quote entity.
/// </summary>
public class PricingResult
{
    public decimal BasePremium        { get; set; }  // SumInsured × Rate × TenureFactor
    public decimal RiskLoading        { get; set; }  // Based on RiskBand
    public decimal OccupationLoading  { get; set; }  // Health/Life only
    public decimal TenureDiscount     { get; set; }  // >12 or >24 months
    public decimal LoyaltyDiscount    { get; set; }  // Renewal policies
    public decimal AgentDiscount      { get; set; }  // Preferred agent
    public decimal TaxAmount          { get; set; }  // 18% GST on adjusted premium
    public decimal TotalPremium       { get; set; }  // Final amount
    public string  PricingNotes       { get; set; } = string.Empty;
}
