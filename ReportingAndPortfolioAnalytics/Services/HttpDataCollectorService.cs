using System.Text.Json;
using ReportingAndPortfolioAnalytics.Domain;
using ReportingAndPortfolioAnalytics.DTOs;
using ReportingAndPortfolioAnalytics.Repositories;

namespace ReportingAndPortfolioAnalytics.Services;

public class HttpDataCollectorService : IHttpDataCollectorService
{
    private readonly IHttpClientFactory _http;
    private readonly IReportRepository  _repo;
    private readonly ILogger<HttpDataCollectorService> _logger;

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public HttpDataCollectorService(
        IHttpClientFactory http,
        IReportRepository  repo,
        ILogger<HttpDataCollectorService> logger)
    {
        _http   = http;
        _repo   = repo;
        _logger = logger;
    }

    // ── Main entry point ─────────────────────────────────────────────────────
    public async Task CollectAndSnapshotAsync(
        string scope, string scopeValue,
        DateTime periodStart, DateTime periodEnd,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Collecting data for {Scope}={ScopeValue}", scope, scopeValue);

        // ── 1. All submissions → filter by product line client-side ────────────
        var allSubs  = await GetListAsync<SubmissionResponseDto>("SubmissionService", "submissions", ct);
        var submissions = allSubs
            .Where(s => s.ProductLine.Equals(scopeValue, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (submissions.Count == 0)
        {
            _logger.LogWarning("No submissions for {ScopeValue}. Snapshot skipped.", scopeValue);
            return;
        }

        var subIds = new HashSet<Guid>(submissions.Select(s => s.SubmissionID));

        // ── 2. Latest quote per submission (parallel) ─────────────────────────
        var quoteTasks = submissions
            .Select(s => GetSingleAsync<QuoteResponseDto>(
                "PricingService", $"quotes/submission/{s.SubmissionID}/latest", ct))
            .ToList();

        // ── 3. All referrals → keep those whose SubmissionID is in our set ────
        var allReferralsTask = GetListAsync<ReferralResponseDto>("RulesService", "referrals", ct);

        // ── 4. Policies for this product line ────────────────────────────────
        // Route: GET /api/policies/product-line/{line}
        var policiesTask = GetListAsync<PolicyResponseDto>(
            "PolicyService", $"policies/product-line/{scopeValue}", ct);

        // ── 5. UW decisions per submission (parallel) ────────────────────────
        var decisionTasks = submissions
            .Select(s => GetListAsync<UWDecisionResponseDto>(
                "UWWorkflowService", $"uw-decisions/{s.SubmissionID}", ct))
            .ToList();

        // ── 6. Risk scores per submission (parallel) ─────────────────────────
        // Route: GET /risk-scores/{submissionId}  returns Band as "Low"|"Medium"|"High"
        var riskTasks = submissions
            .Select(s => GetSingleAsync<RiskScoreResponseDto>(
                "RulesService", $"risk-scores/{s.SubmissionID}", ct))
            .ToList();

        // Wait for all I/O
        await Task.WhenAll(
            Task.WhenAll(quoteTasks),
            allReferralsTask,
            policiesTask,
            Task.WhenAll(decisionTasks),
            Task.WhenAll(riskTasks));

        var quotes      = quoteTasks.Select(t => t.Result).Where(q => q is not null).ToList();
        var referrals   = (await allReferralsTask).Where(r => subIds.Contains(r.SubmissionID)).ToList();
        var policies    = (await policiesTask).Where(p => p.Status == "Active").ToList();
        var allDecisions= decisionTasks.SelectMany(t => t.Result).ToList();
        var riskScores  = riskTasks.Select(t => t.Result).Where(r => r is not null).ToList();

        // ── Compute metrics ──────────────────────────────────────────────────

        var totalQuotes  = submissions.Count;
        var boundCount   = policies.Count;              // bound = active policies
        var totalPremium = quotes.Sum(q => q!.TotalPremium);
        var avgPremium   = boundCount == 0 ? 0m
            : Math.Round(totalPremium / boundCount, 2);

        // TAT: estimate using days since submission created (no CompletedDate in API)
        var tatValues = submissions
            .Select(s => (decimal)(DateTime.UtcNow - s.CreatedDate).TotalHours)
            .ToList();

        // Risk mix: Band "Low" | "Medium" | "High"
        var riskMix = new RiskMixMetric
        {
            Low    = riskScores.Count(r => r!.Band.Equals("Low",    StringComparison.OrdinalIgnoreCase)),
            Medium = riskScores.Count(r => r!.Band.Equals("Medium", StringComparison.OrdinalIgnoreCase)),
            High   = riskScores.Count(r => r!.Band.Equals("High",   StringComparison.OrdinalIgnoreCase)),
        };

        var decidedByCount = allDecisions
            .Where(d => !string.IsNullOrEmpty(d.DecidedBy))
            .Select(d => d.DecidedBy)
            .Distinct()
            .Count();

        var metrics = new ReportMetrics
        {
            Quotes             = totalQuotes,
            BoundPolicies      = boundCount,
            TotalReferrals     = referrals.Count,
            TotalGrossPremium  = totalPremium,
            AvgPremium         = avgPremium,
            TAT_AvgHours       = tatValues.Count > 0 ? Math.Round(tatValues.Average(), 2) : 0,
            TAT_MinHours       = tatValues.Count > 0 ? tatValues.Min() : 0,
            TAT_MaxHours       = tatValues.Count > 0 ? tatValues.Max() : 0,
            TotalDecisions     = allDecisions.Count,
            AvgDecisionsPerUW  = allDecisions.Count == 0 ? 0
                : Math.Round((decimal)allDecisions.Count / Math.Max(decidedByCount, 1), 2),
            RiskMix            = riskMix
        };

        // ── Save / update today's snapshot ───────────────────────────────────
        var existing = await _repo.GetTodaySnapshotAsync(scope, scopeValue);
        if (existing is not null)
        {
            existing.MetricsJSON   = JsonSerializer.Serialize(metrics);
            existing.GeneratedDate = DateTime.UtcNow;
            existing.GeneratedBy   = "http-collector";
        }
        else
        {
            await _repo.AddAsync(new UWReport
            {
                Scope         = scope,
                ScopeValue    = scopeValue,
                PeriodStart   = periodStart,
                PeriodEnd     = periodEnd,
                MetricsJSON   = JsonSerializer.Serialize(metrics),
                GeneratedDate = DateTime.UtcNow,
                GeneratedBy   = "http-collector"
            });
        }

        await _repo.SaveChangesAsync();
        _logger.LogInformation(
            "Snapshot saved: {Scope}={ScopeValue} | quotes={Q} bound={B} refs={R} premium={P}",
            scope, scopeValue, totalQuotes, boundCount, referrals.Count, totalPremium);
    }

    // ── HTTP helpers ──────────────────────────────────────────────────────────

    // Get a list — handles flat array [ ] AND paged envelopes { data/content: [...] }
    private async Task<List<T>> GetListAsync<T>(
        string clientName, string url, CancellationToken ct)
    {
        try
        {
            var client   = _http.CreateClient(clientName);
            var response = await client.GetAsync(url, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("HTTP {Status} from {Client}{Url}",
                    (int)response.StatusCode, clientName, url);
                return new List<T>();
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(json)) return new List<T>();

            var first = json.AsSpan().TrimStart()[0];

            // Flat array
            if (first == '[')
                return JsonSerializer.Deserialize<List<T>>(json, _json) ?? new();

            // Envelope object — try "data" then "content" keys
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            foreach (var key in new[] { "data", "content", "items", "results" })
            {
                if (root.TryGetProperty(key, out var arr) &&
                    arr.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<T>>(arr.GetRawText(), _json) ?? new();
                }
            }

            _logger.LogWarning("Unexpected response shape from {Client}{Url}: {Json}",
                clientName, url, json[..Math.Min(json.Length, 200)]);
            return new List<T>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get list from {Client}{Url}", clientName, url);
            return new List<T>();
        }
    }

    // Get a single item — handles direct object { } AND envelopes { data: { } }
    private async Task<T?> GetSingleAsync<T>(
        string clientName, string url, CancellationToken ct)
        where T : class
    {
        try
        {
            var client   = _http.CreateClient(clientName);
            var response = await client.GetAsync(url, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("HTTP {Status} from {Client}{Url}",
                    (int)response.StatusCode, clientName, url);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(json)) return null;

            var first = json.AsSpan().TrimStart()[0];
            if (first != '{') return null;

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Try unwrapping data envelope
            if (root.TryGetProperty("data", out var data) &&
                data.ValueKind == JsonValueKind.Object)
            {
                return JsonSerializer.Deserialize<T>(data.GetRawText(), _json);
            }

            return JsonSerializer.Deserialize<T>(json, _json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get single from {Client}{Url}", clientName, url);
            return null;
        }
    }
}
