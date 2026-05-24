using System.Net.Http.Json;
using System.Text.Json;

namespace UnderwritingWorkflowAndDecisions.Services
{
    public class HttpSubmissionClientService : ISubmissionClientService
    {
        private readonly HttpClient _http;
        private readonly ILogger<HttpSubmissionClientService> _logger;
        private readonly string _internalKey;

        public HttpSubmissionClientService(HttpClient http, ILogger<HttpSubmissionClientService> logger, IConfiguration config)
        {
            _http        = http;
            _logger      = logger;
            _internalKey = config["InternalServiceKey"] ?? string.Empty;
        }

        public async Task UpdateStatusAsync(Guid submissionId, string status, CancellationToken ct = default)
        {
            try
            {
                var payload = new { Status = status };
                using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/submissions/{submissionId}/status");
                request.Headers.Add("X-Internal-Service-Key", _internalKey);
                request.Content = JsonContent.Create(payload);
                var response = await _http.SendAsync(request, ct);
                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning("Submission status update returned {Code} for {Id}", (int)response.StatusCode, submissionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update submission status for {Id}. Non-blocking.", submissionId);
            }
        }

        public async Task<SubmissionSummaryData?> GetSubmissionAsync(Guid submissionId, CancellationToken ct = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"api/submissions/{submissionId}");
                request.Headers.Add("X-Internal-Service-Key", _internalKey);
                var response = await _http.SendAsync(request, ct);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
                response.EnsureSuccessStatusCode();

                var raw = await response.Content.ReadFromJsonAsync<SubmissionRaw>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);
                if (raw is null) return null;

                return new SubmissionSummaryData
                {
                    ProductLine        = raw.ProductLine ?? string.Empty,
                    SumInsured         = raw.SumInsured,
                    OccupationType     = raw.OccupationType ?? "General",
                    PolicyTenureMonths = raw.PolicyTenureMonths > 0 ? raw.PolicyTenureMonths : 12,
                    Status             = raw.Status ?? string.Empty,
                    InceptionDate      = raw.InceptionDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch submission {Id} for AI summary. Proceeding without it.", submissionId);
                return null;
            }
        }

        private sealed class SubmissionRaw
        {
            public string?   ProductLine        { get; set; }
            public decimal   SumInsured         { get; set; }
            public string?   OccupationType     { get; set; }
            public int       PolicyTenureMonths { get; set; }
            public string?   Status             { get; set; }
            public DateTime  InceptionDate      { get; set; }
        }
    }
}
