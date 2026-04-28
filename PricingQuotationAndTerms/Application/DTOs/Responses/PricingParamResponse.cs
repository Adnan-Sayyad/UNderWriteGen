namespace PricingQuotationAndTerms.Application.DTOs.Responses;

public class PricingParamResponse
{
    public Guid      Id            { get; set; }
    public string    ProductLine   { get; set; } = string.Empty;
    public string    ParamName     { get; set; } = string.Empty;
    public decimal   Value         { get; set; }
    public string    Description   { get; set; } = string.Empty;
    public DateTime  EffectiveFrom { get; set; }
    public DateTime? EffectiveTo   { get; set; }
    public bool      IsActive      { get; set; }
    public DateTime  CreatedAt     { get; set; }
    public DateTime? UpdatedAt     { get; set; }
}
