namespace ReportingAndPortfolioAnalytics.DTOs;

// ── Shapes returned by other microservices' APIs ──────────────────────────────
// Field names match the actual JSON responses from each service.

// From Submission & Intake — GET /api/submissions  (flat array)
// ProductLine values: "Life" | "Health" | "PnC" | "Commercial"
public class SubmissionResponseDto
{
    public Guid   SubmissionID { get; set; }
    public string ProductLine  { get; set; } = string.Empty;
    public string AgentID      { get; set; } = string.Empty;
    public string Status       { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

// From Rules, Scoring & Referral — GET /api/risk-scores/{submissionId}
// Band: "Low" | "Medium" | "High"  (API returns string, NOT integer)
public class RiskScoreResponseDto
{
    public Guid   SubmissionID { get; set; }
    public string Band         { get; set; } = string.Empty;  // "Low" | "Medium" | "High"
    public double ScoreValue   { get; set; }
}

// From Pricing & Quotation — GET /api/quotes/submission/{id}/latest
// Returns a QuoteDto (Id, SubmissionId, TotalPremium, Status)
public class QuoteResponseDto
{
    public Guid    Id           { get; set; }  // PricingService uses "Id" not "QuoteID"
    public Guid    SubmissionId { get; set; }
    public decimal TotalPremium { get; set; }
    public string  Status       { get; set; } = string.Empty;
}

// From Rules, Scoring & Referral — GET /api/referrals  (flat array or paged)
public class ReferralResponseDto
{
    public Guid   ReferralID   { get; set; }
    public Guid   SubmissionID { get; set; }
    public string Status       { get; set; } = string.Empty;
}

// From Policy Binding — GET /api/policies/product-line/{line}
// ProductLine values: "Life" | "Health" | "PnC" | "Commercial"
// Status values:  "Active" | "Cancelled" | "Expired"
public class PolicyResponseDto
{
    public Guid   PolicyID    { get; set; }
    public Guid   SubmissionID{ get; set; }
    public string ProductLine { get; set; } = string.Empty;
    public string Status      { get; set; } = string.Empty;
}

// From UW Workflow — GET /api/uw-decisions/{submissionId}
// Decision values: "Approve" | "Decline" | "Refer" | "MoreInfo"
public class UWDecisionResponseDto
{
    public Guid   DecisionID  { get; set; }
    public Guid   SubmissionID{ get; set; }
    public string Decision    { get; set; } = string.Empty;
    public string DecidedBy   { get; set; } = string.Empty;
}
