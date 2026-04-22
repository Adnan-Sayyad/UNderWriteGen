using System.ComponentModel.DataAnnotations;

namespace PricingQuotationAndTerms.Application.DTOs.Requests;

/// <summary>HTTP request body for PATCH /api/quotes/{quoteId}/terms</summary>
public class UpdateQuoteTermsRequest
{
    [Required]
    public Guid   QuoteId   { get; set; }

    [Required]
    public string TermsJson { get; set; } = string.Empty;
}
