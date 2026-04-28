namespace ReportingAndPortfolioAnalytics.Domain;

/// <summary>
/// Matches spec: UWReport(ReportID, Scope, Metrics, GeneratedDate)
/// Metrics is stored as JSON — flexible for future metric additions.
/// </summary>
public class UWReport
{
	public Guid ReportID { get; set; } = Guid.NewGuid();

	/// <summary>Product | Region | Agent | Period</summary>
	public string Scope { get; set; } = string.Empty;

	/// <summary>The specific value for the scope e.g. "Motor", "South", "AGT001", "2025-Q1"</summary>
	public string ScopeValue { get; set; } = string.Empty;

	/// <summary>
	/// JSON column storing all metrics:
	/// Quotes, HitRatio, TAT, ReferralRate, AvgPremium, RiskMix
	/// </summary>
	public string MetricsJSON { get; set; } = "{}";

	public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

	/// <summary>Who or what triggered this report (user ID or "kafka-consumer")</summary>
	public string GeneratedBy { get; set; } = "system";

	/// <summary>Optional: period boundaries for time-scoped reports</summary>
	public DateTime? PeriodStart { get; set; }
	public DateTime? PeriodEnd { get; set; }
}

/// <summary>
/// Strongly-typed representation of the MetricsJSON column.
/// Serialized/deserialized by the service layer — never stored as columns.
/// </summary>
public class ReportMetrics
{
	// Quote / hit ratio
	public int Quotes { get; set; }
	public int BoundPolicies { get; set; }
	public decimal HitRatio => Quotes == 0 ? 0
		: Math.Round((decimal)BoundPolicies / Quotes * 100, 2);

	// Turnaround time (hours)
	public decimal TAT_AvgHours { get; set; }
	public decimal TAT_MinHours { get; set; }
	public decimal TAT_MaxHours { get; set; }

	// Referral
	public int TotalReferrals { get; set; }
	public decimal ReferralRate => Quotes == 0 ? 0
		: Math.Round((decimal)TotalReferrals / Quotes * 100, 2);

	// Premium
	public decimal AvgPremium { get; set; }
	public decimal TotalGrossPremium { get; set; }

	// Risk mix
	public RiskMixMetric RiskMix { get; set; } = new();

	// UW productivity
	public int TotalDecisions { get; set; }
	public decimal AvgDecisionsPerUW { get; set; }
}

public class RiskMixMetric
{
	public int High { get; set; }
	public int Medium { get; set; }
	public int Low { get; set; }
	public int Total => High + Medium + Low;
}