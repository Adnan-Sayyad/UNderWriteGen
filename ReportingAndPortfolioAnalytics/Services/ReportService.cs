using System.Text.Json;
using ReportingAndPortfolioAnalytics.Domain;
using ReportingAndPortfolioAnalytics.DTOs;
using ReportingAndPortfolioAnalytics.Repositories;

namespace ReportingAndPortfolioAnalytics.Services;

public class ReportService : IReportService
{
	private readonly IReportRepository _repo;

	private static readonly JsonSerializerOptions _json = new()
	{
		PropertyNameCaseInsensitive = true
	};

	public ReportService(IReportRepository repo) => _repo = repo;

	// ── CRUD ──────────────────────────────────────────────────────────────────

	public async Task<IEnumerable<ReportSummaryDto>> GetAllAsync(int page, int pageSize) =>
		(await _repo.GetAllAsync(page, pageSize)).Select(ToSummary);

	public async Task<ReportDetailDto?> GetByIdAsync(Guid id)
	{
		var r = await _repo.GetByIdAsync(id);
		return r is null ? null : ToDetail(r);
	}

	public async Task<IEnumerable<ReportSummaryDto>> GetByScopeAsync(string scope) =>
		(await _repo.GetByScopeAsync(scope)).Select(ToSummary);

	public async Task<ReportDetailDto> GenerateAsync(GenerateReportRequestDto req, string generatedBy)
	{
		var snapshots = (await _repo.QueryAsync(req.Scope, req.PeriodStart, req.PeriodEnd))
			.Where(r => r.ScopeValue == req.ScopeValue)
			.Select(r => ParseMetrics(r.MetricsJSON))
			.ToList();

		var aggregated = new ReportMetrics
		{
			Quotes = snapshots.Sum(m => m.Quotes),
			BoundPolicies = snapshots.Sum(m => m.BoundPolicies),
			TotalReferrals = snapshots.Sum(m => m.TotalReferrals),
			TotalGrossPremium = snapshots.Sum(m => m.TotalGrossPremium),
			AvgPremium = snapshots.Count > 0 ? Math.Round(snapshots.Average(m => m.AvgPremium), 2) : 0,
			TAT_AvgHours = snapshots.Count > 0 ? Math.Round(snapshots.Average(m => m.TAT_AvgHours), 2) : 0,
			TAT_MinHours = snapshots.Count > 0 ? snapshots.Min(m => m.TAT_MinHours) : 0,
			TAT_MaxHours = snapshots.Count > 0 ? snapshots.Max(m => m.TAT_MaxHours) : 0,
			TotalDecisions = snapshots.Sum(m => m.TotalDecisions),
			AvgDecisionsPerUW = snapshots.Count > 0 ? Math.Round(snapshots.Average(m => m.AvgDecisionsPerUW), 2) : 0,
			RiskMix = new RiskMixMetric
			{
				High = snapshots.Sum(m => m.RiskMix.High),
				Medium = snapshots.Sum(m => m.RiskMix.Medium),
				Low = snapshots.Sum(m => m.RiskMix.Low),
			}
		};

		var report = new UWReport
		{
			Scope = req.Scope,
			ScopeValue = req.ScopeValue,
			PeriodStart = req.PeriodStart,
			PeriodEnd = req.PeriodEnd,
			GeneratedDate = DateTime.UtcNow,
			GeneratedBy = generatedBy,
			MetricsJSON = JsonSerializer.Serialize(aggregated)
		};

		await _repo.AddAsync(report);
		await _repo.SaveChangesAsync();
		return ToDetail(report);
	}

	// ── Metric endpoints ──────────────────────────────────────────────────────

	public async Task<IEnumerable<HitRatioDto>> GetHitRatioAsync(string? scope, DateTime? from, DateTime? to) =>
		(await _repo.QueryAsync(scope, from, to))
		.GroupBy(r => r.ScopeValue)
		.Select(g =>
		{
			var m = g.Select(r => ParseMetrics(r.MetricsJSON)).ToList();
			var quotes = m.Sum(x => x.Quotes);
			var bound = m.Sum(x => x.BoundPolicies);
			return new HitRatioDto
			{
				ScopeValue = g.Key,
				Quotes = quotes,
				BoundPolicies = bound,
				HitRatioPercent = quotes == 0 ? 0 : Math.Round((decimal)bound / quotes * 100, 2)
			};
		});

	public async Task<IEnumerable<TatDto>> GetTatAsync(string? scope, DateTime? from, DateTime? to) =>
		(await _repo.QueryAsync(scope, from, to))
		.GroupBy(r => r.ScopeValue)
		.Select(g =>
		{
			// Only use snapshots where TAT was actually collected (AvgHours > 0)
			// Old bad snapshots had zeros; we exclude them so min isn't pulled to 0
			var all = g.Select(r => ParseMetrics(r.MetricsJSON)).ToList();
			var m   = all.Where(x => x.TAT_AvgHours > 0).ToList();
			if (m.Count == 0) m = all; // fall back if no valid snapshots

			var validMin = m.Where(x => x.TAT_MinHours > 0).ToList();
			return new TatDto
			{
				ScopeValue = g.Key,
				AvgHours = Math.Round(m.Average(x => x.TAT_AvgHours), 2),
				MinHours = validMin.Count > 0 ? validMin.Min(x => x.TAT_MinHours) : m.Min(x => x.TAT_MinHours),
				MaxHours = m.Max(x => x.TAT_MaxHours)
			};
		});

	public async Task<IEnumerable<ReferralRateDto>> GetReferralRateAsync(string? scope, DateTime? from, DateTime? to) =>
		(await _repo.QueryAsync(scope, from, to))
		.GroupBy(r => r.ScopeValue)
		.Select(g =>
		{
			var m = g.Select(r => ParseMetrics(r.MetricsJSON)).ToList();
			var quotes = m.Sum(x => x.Quotes);
			var referrals = m.Sum(x => x.TotalReferrals);
			return new ReferralRateDto
			{
				ScopeValue = g.Key,
				Quotes = quotes,
				TotalReferrals = referrals,
				ReferralRatePercent = quotes == 0 ? 0 : Math.Round((decimal)referrals / quotes * 100, 2)
			};
		});

	public async Task<IEnumerable<PremiumDistributionDto>> GetPremiumDistributionAsync(string? scope, DateTime? from, DateTime? to) =>
		(await _repo.QueryAsync(scope, from, to))
		.GroupBy(r => r.ScopeValue)
		.Select(g =>
		{
			var m = g.Select(r => ParseMetrics(r.MetricsJSON)).ToList();
			return new PremiumDistributionDto
			{
				ScopeValue = g.Key,
				TotalGrossPremium = m.Sum(x => x.TotalGrossPremium),
				AvgPremium = Math.Round(m.Average(x => x.AvgPremium), 2),
				PolicyCount = m.Sum(x => x.BoundPolicies)
			};
		});

	public async Task<IEnumerable<RiskMixDto>> GetRiskMixAsync(string? scope, DateTime? from, DateTime? to) =>
		(await _repo.QueryAsync(scope, from, to))
		.GroupBy(r => r.ScopeValue)
		.Select(g =>
		{
			var m = g.Select(r => ParseMetrics(r.MetricsJSON)).ToList();
			var high = m.Sum(x => x.RiskMix.High);
			var medium = m.Sum(x => x.RiskMix.Medium);
			var low = m.Sum(x => x.RiskMix.Low);
			return new RiskMixDto
			{
				ScopeValue = g.Key,
				High = high,
				Medium = medium,
				Low = low,
				Total = high + medium + low
			};
		});

	public async Task<IEnumerable<UWProductivityDto>> GetUWProductivityAsync(string? scope, DateTime? from, DateTime? to) =>
		(await _repo.QueryAsync(scope, from, to))
		.GroupBy(r => r.ScopeValue)
		.Select(g =>
		{
			var m = g.Select(r => ParseMetrics(r.MetricsJSON)).ToList();
			return new UWProductivityDto
			{
				ScopeValue = g.Key,
				TotalDecisions = m.Sum(x => x.TotalDecisions),
				AvgDecisionsPerUW = Math.Round(m.Average(x => x.AvgDecisionsPerUW), 2)
			};
		});

	// ── Helpers (public so Kafka handlers can reuse ParseMetrics) ─────────────

	public static ReportMetrics ParseMetrics(string json)
	{
		try
		{
			return JsonSerializer.Deserialize<ReportMetrics>(json, _json) ?? new ReportMetrics();
		}
		catch
		{
			return new ReportMetrics();
		}
	}

	// ── Mappers ───────────────────────────────────────────────────────────────

	private static ReportSummaryDto ToSummary(UWReport r)
	{
		var m = ParseMetrics(r.MetricsJSON);
		return new ReportSummaryDto
		{
			ReportID = r.ReportID,
			Scope = r.Scope,
			ScopeValue = r.ScopeValue,
			GeneratedDate = r.GeneratedDate,
			PeriodStart = r.PeriodStart,
			PeriodEnd = r.PeriodEnd,
			HitRatio = m.HitRatio,
			TAT_AvgHours = m.TAT_AvgHours,
			ReferralRate = m.ReferralRate,
			AvgPremium = m.AvgPremium
		};
	}

	private static ReportDetailDto ToDetail(UWReport r)
	{
		var m = ParseMetrics(r.MetricsJSON);
		return new ReportDetailDto
		{
			ReportID = r.ReportID,
			Scope = r.Scope,
			ScopeValue = r.ScopeValue,
			GeneratedDate = r.GeneratedDate,
			GeneratedBy = r.GeneratedBy,
			PeriodStart = r.PeriodStart,
			PeriodEnd = r.PeriodEnd,
			HitRatio = m.HitRatio,
			Quotes = m.Quotes,
			BoundPolicies = m.BoundPolicies,
			TAT_AvgHours = m.TAT_AvgHours,
			TAT_MinHours = m.TAT_MinHours,
			TAT_MaxHours = m.TAT_MaxHours,
			ReferralRate = m.ReferralRate,
			TotalReferrals = m.TotalReferrals,
			AvgPremium = m.AvgPremium,
			TotalGrossPremium = m.TotalGrossPremium,
			TotalDecisions = m.TotalDecisions,
			AvgDecisionsPerUW = m.AvgDecisionsPerUW,
			RiskMix = new DTOs.RiskMixDto
			{
				ScopeValue = r.ScopeValue,
				High = m.RiskMix.High,
				Medium = m.RiskMix.Medium,
				Low = m.RiskMix.Low,
				Total = m.RiskMix.Total
			}
		};
	}
}