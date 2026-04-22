namespace PricingQuotationAndTerms.Application.DTOs.External;

/// <summary>
/// Risk evaluation output received from the Rules/Scoring module.
/// Drives loading calculations in PricingService.
/// </summary>
public class RiskScoreInput
{
    public Guid    SubmissionId  { get; set; }
    public decimal ScoreValue    { get; set; }  // 0–100
    public string  Band          { get; set; } = string.Empty; // "Low","Medium","High","Unacceptable"
    public string  ModelVersion  { get; set; } = string.Empty;
}
