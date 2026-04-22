namespace PricingQuotationAndTerms.Contracts.DTOs;

public class RiskScoreDto
{
    public Guid    SubmissionId  { get; set; }
    public decimal ScoreValue    { get; set; }   // 0–100
    public string  Band          { get; set; } = string.Empty; // "Low","Medium","High","Unacceptable"
    public string  ModelVersion  { get; set; } = string.Empty;
}
