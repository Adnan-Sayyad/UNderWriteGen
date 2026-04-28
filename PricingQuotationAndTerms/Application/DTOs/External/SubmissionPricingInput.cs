namespace PricingQuotationAndTerms.Application.DTOs.External;

public class SubmissionPricingInput
{
    public Guid     SubmissionId        { get; set; }
    public Guid     PartyId             { get; set; }
    public Guid     AgentId             { get; set; }
    public string   ProductLine         { get; set; } = string.Empty;
    public decimal  SumInsured          { get; set; }
    public int      PolicyTenureMonths  { get; set; }
    public string   OccupationType      { get; set; } = string.Empty;
    public DateTime InceptionDate       { get; set; }
    public string   CoverageJson        { get; set; } = string.Empty;
    public bool     IsRenewal           { get; set; }
    public bool     IsPreferredAgent    { get; set; }  // ← agent discount flag
}
