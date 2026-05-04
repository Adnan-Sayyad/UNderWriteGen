using System.Net;

namespace PolicyBindingIssuanceAndEndorsements.Services
{
    public class HttpSubmissionClientService : ISubmissionClientService
    {
        private readonly HttpClient _http;
        private readonly ILogger<HttpSubmissionClientService> _logger;

        public HttpSubmissionClientService(HttpClient http, ILogger<HttpSubmissionClientService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<bool> SubmissionExistsAsync(Guid submissionId, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.GetAsync($"submissions/{submissionId}", ct);
                if (response.StatusCode == HttpStatusCode.NotFound) return false;
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Submission service unreachable when validating SubmissionId={Id}. Proceeding.", submissionId);
                return true;
            }
        }
    }
}
