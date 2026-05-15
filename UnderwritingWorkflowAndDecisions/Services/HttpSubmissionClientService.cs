using System.Net.Http.Json;

namespace UnderwritingWorkflowAndDecisions.Services
{
    public class HttpSubmissionClientService : ISubmissionClientService
    {
        private readonly HttpClient _http;
        private readonly ILogger<HttpSubmissionClientService> _logger;
        private readonly string _internalKey;

        public HttpSubmissionClientService(HttpClient http, ILogger<HttpSubmissionClientService> logger, IConfiguration config)
        {
            _http = http;
            _logger = logger;
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
    }
}
