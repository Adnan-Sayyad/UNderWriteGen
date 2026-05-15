using System.Net.Http.Json;

namespace PolicyBindingIssuanceAndEndorsements.Services
{
    public class HttpComplianceClientService : IComplianceClientService
    {
        private readonly HttpClient _http;
        private readonly ILogger<HttpComplianceClientService> _logger;

        public HttpComplianceClientService(HttpClient http, ILogger<HttpComplianceClientService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task CreateChecklistAsync(Guid submissionId, CancellationToken ct = default)
        {
            try
            {
                var payload = new
                {
                    SubmissionId = submissionId,
                    ItemsJson    = "[]",
                    Status       = "Pending"
                };
                var response = await _http.PostAsJsonAsync("api/compliance-checklists", payload, ct);
                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning("Compliance checklist creation returned {Code} for submission {Id}",
                        (int)response.StatusCode, submissionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create compliance checklist for submission {Id}. Non-blocking.", submissionId);
            }
        }
    }
}
