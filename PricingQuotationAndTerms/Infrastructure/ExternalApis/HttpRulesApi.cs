using System.Net.Http.Json;
using System.Text.Json;
using PricingQuotationAndTerms.Contracts.DTOs;
using PricingQuotationAndTerms.Contracts.Interfaces;

namespace PricingQuotationAndTerms.Infrastructure.ExternalApis;

/// <summary>
/// PRODUCTION implementation — calls the real Rules/Scoring microservice.
/// Band is returned as a string ("Low" | "Medium" | "High") by the Rules service.
/// </summary>
public class HttpRulesApi : IRulesApi
{
    private readonly HttpClient _http;
    private readonly ILogger<HttpRulesApi> _logger;

    public HttpRulesApi(HttpClient http, ILogger<HttpRulesApi> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task<RiskScoreDto?> GetRiskScoreAsync(Guid submissionId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync($"risk-scores/{submissionId}", ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Risk score not found for SubmissionId={Id}", submissionId);
                return null;
            }

            response.EnsureSuccessStatusCode();

            // Deserialize Rules service response — Band is a string "Low"|"Medium"|"High"
            var raw = await response.Content.ReadFromJsonAsync<RiskScoreRaw>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

            if (raw is null) return null;

            string bandName = string.IsNullOrWhiteSpace(raw.Band) ? "Medium" : raw.Band;

            return new RiskScoreDto
            {
                SubmissionId = raw.SubmissionID,
                ScoreValue   = (decimal)raw.ScoreValue,
                Band         = bandName,
                ModelVersion = raw.ModelVersion ?? "v1.0"
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach Rules service for SubmissionId={Id}", submissionId);
            throw new InvalidOperationException(
                "Rules/Scoring service is unavailable. Cannot calculate risk score.", ex);
        }
    }

    // Mirrors RiskScoreResponseDto from RulesScoringAndReferralMatrix service
    private sealed class RiskScoreRaw
    {
        public Guid    RiskScoreID  { get; set; }
        public Guid    SubmissionID { get; set; }
        public string? ModelVersion { get; set; }
        public double  ScoreValue   { get; set; }
        public string  Band         { get; set; } = string.Empty;  // "Low" | "Medium" | "High"
        public DateTime ScoredDate  { get; set; }
    }
}
