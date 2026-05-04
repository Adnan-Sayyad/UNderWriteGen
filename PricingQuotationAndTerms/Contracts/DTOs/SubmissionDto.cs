namespace PricingQuotationAndTerms.Contracts.DTOs;

/// <summary>
/// Data received FROM the Submission microservice.
/// In production this is deserialized from an HTTP response.
/// For local dev, FakeSubmissionApi returns this directly.
/// </summary>
public class SubmissionDto
{
    public Guid     Id                  { get; set; }
    public string   PartyId             { get; set; } = string.Empty; // e.g. "PTY-20260501-0001"
    public string   AgentId             { get; set; } = string.Empty; // e.g. "AGT-20260501-0001"
    public string   ProductLine         { get; set; } = string.Empty; // "Life","Health","PnC","Commercial"
    public decimal  SumInsured          { get; set; }
    public int      PolicyTenureMonths  { get; set; }  // 12 = 1 year
    public string   OccupationType      { get; set; } = string.Empty;
    public DateTime InceptionDate       { get; set; }
    public int      RiskScore           { get; set; }  // 0–100
    public string   RiskBand            { get; set; } = string.Empty; // "Low","Medium","High","Unacceptable"
    public string   CoverageJson        { get; set; } = string.Empty;
    public bool     IsRenewal           { get; set; }  // for loyalty discount
}
