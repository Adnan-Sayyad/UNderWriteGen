using System.ComponentModel.DataAnnotations;

namespace PricingQuotationAndTerms.Application.DTOs.Requests;

public class CreatePricingParamRequest
{
    [Required][MaxLength(50)]
    public string  ProductLine   { get; set; } = string.Empty;

    [Required][MaxLength(100)]
    public string  ParamName     { get; set; } = string.Empty;

    [Required][Range(0, double.MaxValue)]
    public decimal Value         { get; set; }

    [Required][MaxLength(250)]
    public string  Description   { get; set; } = string.Empty;

    public DateTime  EffectiveFrom { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveTo   { get; set; }  // null = no expiry
}
