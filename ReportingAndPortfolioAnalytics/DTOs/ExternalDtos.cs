namespace ReportingAndPortfolioAnalytics.DTOs;

// ── Shapes returned by other microservices' APIs ──────────────────────────────
// Field names must match exactly what each service returns in their JSON.
// Coordinate with each team to confirm these match their response DTOs.

// From Submission & Intake — GET /api/submissions?status=Quoted&status=Declined
public class SubmissionResponseDto
{
	public Guid SubmissionID { get; set; }
	public string ProductLine { get; set; } = string.Empty;
	public string Region { get; set; } = string.Empty;
	public string AgentID { get; set; } = string.Empty;
	public string Status { get; set; } = string.Empty;
	public DateTime CreatedDate { get; set; }
	public DateTime? CompletedDate { get; set; }
}

// From Rules, Scoring & Referral — GET /api/risk-scores?submissionId=...
public class RiskScoreResponseDto
{
	public Guid SubmissionID { get; set; }
	public string Band { get; set; } = string.Empty; // Low | Medium | High
}

// From Pricing & Quotation — GET /api/quotes?status=Accepted
public class QuoteResponseDto
{
	public Guid QuoteID { get; set; }
	public Guid SubmissionID { get; set; }
	public string ProductLine { get; set; } = string.Empty;
	public string AgentID { get; set; } = string.Empty;
	public decimal TotalPremium { get; set; }
	public string Status { get; set; } = string.Empty;
}

// From Rules, Scoring & Referral — GET /api/referrals?status=Pending
public class ReferralResponseDto
{
	public Guid ReferralID { get; set; }
	public Guid SubmissionID { get; set; }
	public string ProductLine { get; set; } = string.Empty;
	public string Status { get; set; } = string.Empty;
}

// From Policy Binding — GET /api/policies?status=Active
public class PolicyResponseDto
{
	public Guid PolicyID { get; set; }
	public Guid SubmissionID { get; set; }
	public string ProductLine { get; set; } = string.Empty;
	public string Region { get; set; } = string.Empty;
	public string AgentID { get; set; } = string.Empty;
	public decimal TotalPremium { get; set; }
	public string Status { get; set; } = string.Empty;
}

// From UW Workflow — GET /api/uw-decisions
public class UWDecisionResponseDto
{
	public Guid DecisionID { get; set; }
	public Guid SubmissionID { get; set; }
	public string Decision { get; set; } = string.Empty;
	public string DecidedBy { get; set; } = string.Empty;
	public string ProductLine { get; set; } = string.Empty;
}