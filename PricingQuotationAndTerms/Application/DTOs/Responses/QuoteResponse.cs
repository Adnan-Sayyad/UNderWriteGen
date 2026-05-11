namespace PricingQuotationAndTerms.Application.DTOs.Responses;

/// <summary>HTTP response returned by the QuoteController to the API client (Postman / frontend).</summary>
public class QuoteResponse
{
    /// <summary>Human-readable reference number e.g. QUO-2026-04287. For display only — use QuoteId for API calls.</summary>
    public string   QuoteRef     { get; set; } = string.Empty;
    public Guid     QuoteId      { get; set; }
    public Guid     SubmissionId { get; set; }
    public int      VersionNo    { get; set; }
    public decimal  BasePremium  { get; set; }
    public decimal  TotalPremium { get; set; }
    public DateTime ValidUntil   { get; set; }
    public string   Status       { get; set; } = string.Empty;
    public string   LoadingsJson { get; set; } = string.Empty;
    public string   DiscountsJson{ get; set; } = string.Empty;
    public string   TaxesJson    { get; set; } = string.Empty;
}
