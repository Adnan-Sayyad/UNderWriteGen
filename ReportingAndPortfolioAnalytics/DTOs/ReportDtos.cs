namespace ReportingAndPortfolioAnalytics.DTOs;

// ── List / summary ────────────────────────────────────────────────────────────

public class ReportSummaryDto
{
	public Guid ReportID { get; set; }
	public string Scope { get; set; } = string.Empty;
	public string ScopeValue { get; set; } = string.Empty;
	public DateTime GeneratedDate { get; set; }
	public DateTime? PeriodStart { get; set; }
	public DateTime? PeriodEnd { get; set; }

	// Key headline metrics shown in list view
	public decimal HitRatio { get; set; }
	public decimal TAT_AvgHours { get; set; }
	public decimal ReferralRate { get; set; }
	public decimal AvgPremium { get; set; }
}

// ── Full detail ───────────────────────────────────────────────────────────────

public class ReportDetailDto : ReportSummaryDto
{
	public string GeneratedBy { get; set; } = string.Empty;

	// All metrics expanded
	public int Quotes { get; set; }
	public int BoundPolicies { get; set; }
	public decimal TAT_MinHours { get; set; }
	public decimal TAT_MaxHours { get; set; }
	public int TotalReferrals { get; set; }
	public decimal TotalGrossPremium { get; set; }
	public RiskMixDto RiskMix { get; set; } = new();
	public int TotalDecisions { get; set; }
	public decimal AvgDecisionsPerUW { get; set; }
}

// ── Generate request ──────────────────────────────────────────────────────────

public class GenerateReportRequestDto
{
	public string Scope { get; set; } = string.Empty;
	public string ScopeValue { get; set; } = string.Empty;
	public DateTime PeriodStart { get; set; }
	public DateTime PeriodEnd { get; set; }
}

// ── Metric-specific response DTOs (one per endpoint) ─────────────────────────

public class HitRatioDto
{
	public string ScopeValue { get; set; } = string.Empty;
	public int Quotes { get; set; }
	public int BoundPolicies { get; set; }
	public decimal HitRatioPercent { get; set; }
}

public class TatDto
{
	public string ScopeValue { get; set; } = string.Empty;
	public decimal AvgHours { get; set; }
	public decimal MinHours { get; set; }
	public decimal MaxHours { get; set; }
}

public class ReferralRateDto
{
	public string ScopeValue { get; set; } = string.Empty;
	public int Quotes { get; set; }
	public int TotalReferrals { get; set; }
	public decimal ReferralRatePercent { get; set; }
}

public class PremiumDistributionDto
{
	public string ScopeValue { get; set; } = string.Empty;
	public decimal TotalGrossPremium { get; set; }
	public decimal AvgPremium { get; set; }
	public int PolicyCount { get; set; }
}

public class RiskMixDto
{
	public string ScopeValue { get; set; } = string.Empty;
	public int High { get; set; }
	public int Medium { get; set; }
	public int Low { get; set; }
	public int Total { get; set; }
}

public class UWProductivityDto
{
	public string ScopeValue { get; set; } = string.Empty;
	public int TotalDecisions { get; set; }
	public decimal AvgDecisionsPerUW { get; set; }
}