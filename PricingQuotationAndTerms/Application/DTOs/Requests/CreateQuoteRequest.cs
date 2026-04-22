using System.ComponentModel.DataAnnotations;

namespace PricingQuotationAndTerms.Application.DTOs.Requests;

/// <summary>HTTP request body for POST /api/quotes/generate</summary>
public class CreateQuoteRequest
{
    [Required]
    public Guid   SubmissionId  { get; set; }

    public bool   ApplyDiscounts { get; set; } = true;
    public bool   ApplyTaxes     { get; set; } = true;

    [Required]
    [MaxLength(100)]
    public string RequestedBy   { get; set; } = string.Empty;  // username / agent code
}
