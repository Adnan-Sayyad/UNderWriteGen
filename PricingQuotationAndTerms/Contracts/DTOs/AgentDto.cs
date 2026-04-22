namespace PricingQuotationAndTerms.Contracts.DTOs;

public class AgentDto
{
    public Guid    AgentId          { get; set; }
    public string  ProducerCode     { get; set; } = string.Empty;
    public string  Region           { get; set; } = string.Empty;
    public bool    IsPreferredAgent { get; set; }   // preferred agents pass discount to customer
    public decimal CommissionRate   { get; set; }   // e.g. 0.05 = 5%
}
