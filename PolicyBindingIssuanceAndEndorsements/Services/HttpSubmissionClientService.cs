using System.Net;
using System.Net.Http.Json;

namespace PolicyBindingIssuanceAndEndorsements.Services
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

        public async Task<bool> SubmissionExistsAsync(Guid submissionId, CancellationToken ct = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"submissions/{submissionId}");
                request.Headers.Add("X-Internal-Service-Key", _internalKey);
                var response = await _http.SendAsync(request, ct);
                if (response.StatusCode == HttpStatusCode.NotFound) return false;
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Submission service unreachable when validating SubmissionId={Id}. Proceeding.", submissionId);
                return true;
            }
        }

        public async Task UpdateStatusAsync(Guid submissionId, string status, CancellationToken ct = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Patch, $"submissions/{submissionId}/status");
                request.Headers.Add("X-Internal-Service-Key", _internalKey);
                request.Content = JsonContent.Create(new { Status = status });
                await _http.SendAsync(request, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update submission {Id} status to {Status}.", submissionId, status);
            }
        }
    }
}
