using System.Net.Http.Json;
using System.Text.Json;
using RulesScoringAndReferralMatrix.DTOs;

namespace RulesScoringAndReferralMatrix.Services
{
    public class HttpSubmissionClientService : ISubmissionClientService
    {
        private readonly HttpClient _http;
        private readonly ILogger<HttpSubmissionClientService> _logger;
        private readonly string _internalKey;

        public HttpSubmissionClientService(
            HttpClient http,
            ILogger<HttpSubmissionClientService> logger,
            IConfiguration config)
        {
            _http        = http;
            _logger      = logger;
            _internalKey = config["InternalServiceKey"] ?? string.Empty;
        }

        public async Task<SubmissionDataDto?> GetSubmissionAsync(Guid submissionId, CancellationToken ct = default)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"submissions/{submissionId}");
                req.Headers.Add("X-Internal-Service-Key", _internalKey);
                var response = await _http.SendAsync(req, ct);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Submission not found: {Id}", submissionId);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var raw = await response.Content.ReadFromJsonAsync<SubmissionRaw>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

                if (raw is null) return null;

                // SumInsured: prefer questionnaire-enriched field, fall back to CoverageJSON
                decimal sumInsured = raw.SumInsured;
                if (sumInsured <= 0 && !string.IsNullOrWhiteSpace(raw.CoverageJSON))
                {
                    try
                    {
                        var coverage = JsonSerializer.Deserialize<JsonElement>(raw.CoverageJSON);
                        if (coverage.TryGetProperty("sumInsured", out var si))
                            sumInsured = si.GetDecimal();
                    }
                    catch { /* leave as 0 */ }
                }

                return new SubmissionDataDto
                {
                    SubmissionId       = raw.SubmissionID,
                    ProductLine        = raw.ProductLine ?? string.Empty,
                    SumInsured         = sumInsured,
                    PolicyTenureMonths = raw.PolicyTenureMonths > 0 ? raw.PolicyTenureMonths : 12,
                    OccupationType     = !string.IsNullOrWhiteSpace(raw.OccupationType)
                                         ? raw.OccupationType
                                         : "General"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to reach Submission service for Id={Id}", submissionId);
                throw new InvalidOperationException(
                    "Submission service is unavailable. Cannot calculate risk score.", ex);
            }
        }

        // Mirrors SubmissionResponseDto from SubmissionAndIntake service.
        private sealed class SubmissionRaw
        {
            public Guid     SubmissionID       { get; set; }
            public string?  ProductLine        { get; set; }
            public string?  CoverageJSON       { get; set; }
            public decimal  SumInsured         { get; set; }
            public int      PolicyTenureMonths { get; set; }
            public string?  OccupationType     { get; set; }
        }
    }
}
