using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PricingQuotationAndTerms.Contracts.DTOs;
using PricingQuotationAndTerms.Contracts.Interfaces;

namespace PricingQuotationAndTerms.Infrastructure.ExternalApis;

/// <summary>
/// PRODUCTION implementation — calls the real Submission microservice at port 8083.
/// Maps the Submission service response shape to the internal SubmissionDto.
/// </summary>
public class HttpSubmissionApi : ISubmissionApi
{
    private readonly HttpClient _http;
    private readonly ILogger<HttpSubmissionApi> _logger;

    // ProductLine enum values from SubmissionAndIntake service
    private static readonly string[] ProductLineNames = ["Life", "Health", "PnC", "Commercial"];

    public HttpSubmissionApi(HttpClient http, ILogger<HttpSubmissionApi> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task<SubmissionDto?> GetSubmissionByIdAsync(Guid submissionId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync($"submissions/{submissionId}", ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Submission not found: {Id}", submissionId);
                return null;
            }

            response.EnsureSuccessStatusCode();

            // Deserialize the actual Submission service response shape
            var raw = await response.Content.ReadFromJsonAsync<SubmissionRaw>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

            if (raw is null) return null;

            // Extract SumInsured from coverageJSON e.g. {"sumInsured": 500000, ...}
            decimal sumInsured = 0;
            try
            {
                var coverage = JsonSerializer.Deserialize<JsonElement>(raw.CoverageJSON ?? "{}");
                if (coverage.TryGetProperty("sumInsured", out var si))
                    sumInsured = si.GetDecimal();
            }
            catch { /* leave as 0 if parse fails */ }

            // Map ProductLine integer enum to string name
            string productLine = (raw.ProductLine >= 0 && raw.ProductLine < ProductLineNames.Length)
                ? ProductLineNames[raw.ProductLine]
                : "Life";

            return new SubmissionDto
            {
                Id                 = raw.SubmissionID,
                PartyId            = raw.PartyID,
                AgentId            = raw.AgentID,
                ProductLine        = productLine,
                SumInsured         = sumInsured,
                PolicyTenureMonths = 12,          // default — not stored on Submission
                OccupationType     = "General",   // default — not stored on Submission
                InceptionDate      = raw.InceptionDate,
                CoverageJson       = raw.CoverageJSON ?? string.Empty,
                IsRenewal          = false
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach Submission service for Id={Id}", submissionId);
            throw new InvalidOperationException(
                "Submission service is unavailable. Cannot retrieve submission.", ex);
        }
    }

    /// <summary>
    /// Mirrors the actual SubmissionResponseDto returned by the SubmissionAndIntake service.
    /// </summary>
    private sealed class SubmissionRaw
    {
        public Guid     SubmissionID  { get; set; }
        public string   PartyID       { get; set; } = string.Empty;
        public string   AgentID       { get; set; } = string.Empty;
        public int      ProductLine   { get; set; }   // 0=Life,1=Health,2=PnC,3=Commercial
        public string?  CoverageJSON  { get; set; }
        public DateTime InceptionDate { get; set; }
        public DateTime CreatedDate   { get; set; }
        public int      Status        { get; set; }
    }
}
