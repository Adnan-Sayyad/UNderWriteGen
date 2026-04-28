using System.ComponentModel.DataAnnotations;

namespace PricingQuotationAndTerms.Application.DTOs.Requests;

public class UpdateQuoteStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty; // "Draft","Presented","Accepted","Declined","Expired"

    [MaxLength(250)]
    public string? Reason { get; set; }  // Optional — reason for status change
}
