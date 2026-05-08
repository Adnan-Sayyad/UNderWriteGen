using System.Text.Json;
using ReportingAndPortfolioAnalytics.Domain;
using ReportingAndPortfolioAnalytics.DTOs;
using ReportingAndPortfolioAnalytics.Repositories;

namespace ReportingAndPortfolioAnalytics.Services;

public class HttpDataCollectorService : IHttpDataCollectorService
{
	private readonly IHttpClientFactory _http;
	private readonly IReportRepository _repo;
	private readonly ILogger<HttpDataCollectorService> _logger;

	private static readonly JsonSerializerOptions _json = new()
	{
		PropertyNameCaseInsensitive = true
	};

	public HttpDataCollectorService(
		IHttpClientFactory http,
		IReportRepository repo,
		ILogger<HttpDataCollectorService> logger)
	{
		_http = http;
		_repo = repo;
		_logger = logger;
	}

	public async Task CollectAndSnapshotAsync(
		string scope, string scopeValue,
		DateTime periodStart, DateTime periodEnd,
		CancellationToken ct = default)
	{
		_logger.LogInformation("Collecting data for {Scope}={ScopeValue} {From} → {To}",
			scope, scopeValue, periodStart, periodEnd);

		var from = periodStart.ToString("yyyy-MM-dd");
		var to = periodEnd.ToString("yyyy-MM-dd");

		// ── Fetch from each service in parallel ───────────────────────────────
		var submissionsTask = GetAsync<List<SubmissionResponseDto>>(
			"SubmissionService",
			$"/submissions?productLine={scopeValue}&from={from}&to={to}", ct);

		var quotesTask = GetAsync<List<QuoteResponseDto>>(
			"PricingService",
			$"/quotes?productLine={scopeValue}&status=Accepted&from={from}&to={to}", ct);

		var referralsTask = GetAsync<List<ReferralResponseDto>>(
			"RulesService",
			$"/referrals?productLine={scopeValue}&from={from}&to={to}", ct);

		var policiesTask = GetAsync<List<PolicyResponseDto>>(
			"PolicyService",
			$"/policies?productLine={scopeValue}&status=Active&from={from}&to={to}", ct);

		var decisionsTask = GetAsync<List<UWDecisionResponseDto>>(
			"UWWorkflowService",
			$"/uw-decisions?productLine={scopeValue}&from={from}&to={to}", ct);

		await Task.WhenAll(submissionsTask, quotesTask, referralsTask, policiesTask, decisionsTask);

		var submissions = submissionsTask.Result ?? new();
		var quotes = quotesTask.Result ?? new();
		var referrals = referralsTask.Result ?? new();
		var policies = policiesTask.Result ?? new();
		var decisions = decisionsTask.Result ?? new();

		// ── Compute metrics ───────────────────────────────────────────────────

		// Hit ratio
		var totalQuotes = submissions.Count;
		var boundCount = quotes.Count(q => q.Status == "Accepted");

		// TAT — only finalised submissions
		var finalised = submissions
			.Where(s => s.Status is "Quoted" or "Declined" && s.CompletedDate.HasValue)
			.ToList();

		var tatValues = finalised
			.Select(s => (decimal)(s.CompletedDate!.Value - s.CreatedDate).TotalHours)
			.ToList();

		// Risk mix — fetch risk scores for each submission
		var riskScoreTasks = finalised
			.Select(s => GetAsync<RiskScoreResponseDto>(
				"RulesService",
				$"/risk-scores/{s.SubmissionID}", ct))
			.ToList();

		await Task.WhenAll(riskScoreTasks);
		var riskScores = riskScoreTasks
			.Select(t => t.Result)
			.Where(r => r is not null)
			.ToList();

		// Premium
		var totalPremium = quotes.Sum(q => q.TotalPremium);
		var avgPremium = boundCount == 0 ? 0 : Math.Round(totalPremium / boundCount, 2);

		// Build metrics object
		var metrics = new ReportMetrics
		{
			Quotes = totalQuotes,
			BoundPolicies = boundCount,
			TotalReferrals = referrals.Count,
			TotalGrossPremium = totalPremium,
			AvgPremium = avgPremium,
			TAT_AvgHours = tatValues.Count > 0 ? Math.Round(tatValues.Average(), 2) : 0,
			TAT_MinHours = tatValues.Count > 0 ? tatValues.Min() : 0,
			TAT_MaxHours = tatValues.Count > 0 ? tatValues.Max() : 0,
			TotalDecisions = decisions.Count,
			AvgDecisionsPerUW = decisions.Count == 0 ? 0
				: Math.Round((decimal)decisions.Count
					/ Math.Max(decisions.Select(d => d.DecidedBy).Distinct().Count(), 1), 2),
			RiskMix = new RiskMixMetric
			{
				High = riskScores.Count(r => r!.Band == "High"),
				Medium = riskScores.Count(r => r!.Band == "Medium"),
				Low = riskScores.Count(r => r!.Band == "Low"),
			}
		};

		// ── Guard: skip saving if all upstream services returned empty data ──────
		if (totalQuotes == 0 && quotes.Count == 0 && referrals.Count == 0 && decisions.Count == 0)
		{
			_logger.LogWarning(
				"All upstream services returned empty data for {Scope}={ScopeValue}. Snapshot not saved.",
				scope, scopeValue);
			return;
		}

		// ── Save snapshot ─────────────────────────────────────────────────────
		var existing = await _repo.GetTodaySnapshotAsync(scope, scopeValue);

		if (existing is not null)
		{
			// Overwrite today's snapshot with freshly collected data
			existing.MetricsJSON = JsonSerializer.Serialize(metrics);
			existing.GeneratedDate = DateTime.UtcNow;
			existing.GeneratedBy = "http-collector";
		}
		else
		{
			await _repo.AddAsync(new UWReport
			{
				Scope = scope,
				ScopeValue = scopeValue,
				PeriodStart = periodStart,
				PeriodEnd = periodEnd,
				MetricsJSON = JsonSerializer.Serialize(metrics),
				GeneratedDate = DateTime.UtcNow,
				GeneratedBy = "http-collector"
			});
		}

		await _repo.SaveChangesAsync();
		_logger.LogInformation("Snapshot saved for {Scope}={ScopeValue}", scope, scopeValue);
	}

	// ── Helper — safe HTTP GET with null on failure ───────────────────────────

	private async Task<T?> GetAsync<T>(string clientName, string url, CancellationToken ct)
	{
		try
		{
			var client = _http.CreateClient(clientName);
			var response = await client.GetAsync(url, ct);

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogWarning("HTTP {Status} from {Client}{Url}",
					(int)response.StatusCode, clientName, url);
				return default;
			}

			var json = await response.Content.ReadAsStringAsync(ct);
			return JsonSerializer.Deserialize<T>(json, _json);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to call {Client}{Url}", clientName, url);
			return default;
		}
	}
}