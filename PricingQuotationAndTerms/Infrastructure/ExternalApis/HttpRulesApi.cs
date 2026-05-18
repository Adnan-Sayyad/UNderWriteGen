using System.Net.Http.Json;
using System.Text.Json;
using PricingQuotationAndTerms.Contracts.DTOs;
using PricingQuotationAndTerms.Contracts.Interfaces;

namespace PricingQuotationAndTerms.Infrastructure.ExternalApis;

/// <summary>
/// PRODUCTION implementation — calls the real Rules/Scoring microservice.
/// Rules service uses JsonStringEnumConverter, so Band arrives as "Low"/"Medium"/"High".
/// </summary>
public class HttpRulesApi : IRulesApi
{
    private readonly HttpClient _http;
    private readonly ILogger<HttpRulesApi> _logger;
    private readonly string _internalKey;

    // Valid band names emitted by the Rules service (JsonStringEnumConverter)
    private static readonly HashSet<string> ValidBands = ["Low", "Medium", "High"];

    public HttpRulesApi(HttpClient http, ILogger<HttpRulesApi> logger, IConfiguration config)
    {
        _http        = http;
        _logger      = logger;
        _internalKey = config["InternalServiceKey"] ?? string.Empty;
    }

    public async Task<RiskScoreDto?> GetRiskScoreAsync(Guid submissionId, CancellationToken ct = default)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, $"risk-scores/{submissionId}");
            req.Headers.Add("X-Internal-Service-Key", _internalKey);
            var response = await _http.SendAsync(req, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Risk score not found for SubmissionId={Id}", submissionId);
                return null;
            }

            response.EnsureSuccessStatusCode();

            // Deserialize Rules service response — Band is a string enum ("Low"/"Medium"/"High")
            var raw = await response.Content.ReadFromJsonAsync<RiskScoreRaw>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

            if (raw is null) return null;

            // Fall back to "Medium" if the value is missing or unrecognised
            string bandName = (!string.IsNullOrWhiteSpace(raw.Band) && ValidBands.Contains(raw.Band))
                ? raw.Band
                : "Medium";

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

    // Mirrors RiskScoreResponseDto from RulesScoringAndReferralMatrix service.
    // Band is serialised as a string by JsonStringEnumConverter on the Rules service side.
    private sealed class RiskScoreRaw
    {
        public Guid    RiskScoreID  { get; set; }
        public Guid    SubmissionID { get; set; }
        public string? ModelVersion { get; set; }
        public double  ScoreValue   { get; set; }
        public string  Band         { get; set; } = string.Empty;  // "Low", "Medium", "High"
        public DateTime ScoredDate  { get; set; }
    }
}
