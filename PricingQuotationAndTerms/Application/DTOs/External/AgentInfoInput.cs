namespace PricingQuotationAndTerms.Application.DTOs.External;

/// <summary>
/// Agent details from the Distribution module.
/// Preferred agents earn an additional discount for the customer.
/// </summary>
public class AgentInfoInput
{
    public Guid    AgentId          { get; set; }
    public string  Region           { get; set; } = string.Empty;
    public string  ProducerCode     { get; set; } = string.Empty;
    public bool    IsPreferredAgent { get; set; }
    public decimal CommissionRate   { get; set; }  // e.g. 0.05 = 5%
}
