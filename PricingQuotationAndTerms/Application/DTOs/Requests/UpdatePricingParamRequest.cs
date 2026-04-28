using System.ComponentModel.DataAnnotations;

namespace PricingQuotationAndTerms.Application.DTOs.Requests;

public class UpdatePricingParamRequest
{
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Value must be non-negative.")]
    public decimal Value       { get; set; }

    [Required]
    [MaxLength(250)]
    public string Description  { get; set; } = string.Empty;
}
