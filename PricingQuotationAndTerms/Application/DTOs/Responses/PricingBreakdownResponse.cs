namespace PricingQuotationAndTerms.Application.DTOs.Responses;

/// <summary>Detailed pricing breakdown for display on the UI quote screen.</summary>
public class PricingBreakdownResponse
{
    public decimal BasePremium       { get; set; }
    public decimal RiskLoading       { get; set; }
    public decimal OccupationLoading { get; set; }
    public decimal TenureDiscount    { get; set; }
    public decimal LoyaltyDiscount   { get; set; }
    public decimal AgentDiscount     { get; set; }
    public decimal TaxAmount         { get; set; }
    public decimal TotalPremium      { get; set; }
    public string  PricingNotes      { get; set; } = string.Empty;
}
