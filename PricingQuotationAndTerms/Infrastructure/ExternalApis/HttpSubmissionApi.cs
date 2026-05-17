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


    private readonly string _internalKey;

    public HttpSubmissionApi(HttpClient http, ILogger<HttpSubmissionApi> logger, IConfiguration config)
    {
        _http        = http;
        _logger      = logger;
        _internalKey = config["InternalServiceKey"] ?? string.Empty;
    }

    public async Task<SubmissionDto?> GetSubmissionByIdAsync(Guid submissionId, CancellationToken ct = default)
    {
        try
        {
            using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"submissions/{submissionId}");
            getRequest.Headers.Add("X-Internal-Service-Key", _internalKey);
            var response = await _http.SendAsync(getRequest, ct);

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

            return new SubmissionDto
            {
                Id                 = raw.SubmissionID,
                PartyId            = raw.PartyID,
                AgentId            = raw.AgentID,
                ProductLine        = raw.ProductLine,
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

    public async Task UpdateSubmissionStatusAsync(Guid submissionId, string status, CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Patch, $"submissions/{submissionId}/status");
            request.Headers.Add("X-Internal-Service-Key", _internalKey);
            request.Content = JsonContent.Create(new { Status = status });
            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("Submission status update returned {Code} for {Id}", (int)response.StatusCode, submissionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update submission status for {Id}. Non-blocking.", submissionId);
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
        public string   ProductLine   { get; set; } = string.Empty;  // "Life","Health","PnC","Commercial"
        public string?  CoverageJSON  { get; set; }
        public DateTime InceptionDate { get; set; }
        public DateTime CreatedDate   { get; set; }
        public string   Status        { get; set; } = string.Empty;
    }
}
