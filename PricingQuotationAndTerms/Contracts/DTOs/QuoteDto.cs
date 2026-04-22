namespace PricingQuotationAndTerms.Contracts.DTOs;

/// <summary>
/// Shared DTO exposed via IQuoteApi to other modules (e.g. PolicyBinding, Underwriting).
/// Keep this stable — changing it is a breaking change for consumers.
/// </summary>
public class QuoteDto
{
    public Guid     Id           { get; set; }
    public Guid     SubmissionId { get; set; }
    public int      VersionNo    { get; set; }
    public decimal  BasePremium  { get; set; }
    public decimal  TotalPremium { get; set; }
    public string   LoadingsJson { get; set; } = string.Empty;
    public string   DiscountsJson{ get; set; } = string.Empty;
    public string   TaxesJson    { get; set; } = string.Empty;
    public string   TermsJson    { get; set; } = string.Empty;
    public DateTime ValidUntil   { get; set; }
    public string   Status       { get; set; } = string.Empty;
}
