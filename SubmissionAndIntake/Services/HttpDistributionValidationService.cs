using System.Net;

namespace SubmissionAndIntake.Services
{
    public class HttpDistributionValidationService : IDistributionValidationService
    {
        private readonly HttpClient _http;
        private readonly ILogger<HttpDistributionValidationService> _logger;

        public HttpDistributionValidationService(HttpClient http, ILogger<HttpDistributionValidationService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<bool> AgentExistsAsync(string agentId, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.GetAsync($"agents/{agentId}", ct);
                if (response.StatusCode == HttpStatusCode.NotFound) return false;
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Distribution service unreachable when validating AgentId={Id}. Proceeding.", agentId);
                return true;
            }
        }

        public async Task<bool> PartyExistsAsync(string partyId, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.GetAsync($"customerparties/{partyId}", ct);
                if (response.StatusCode == HttpStatusCode.NotFound) return false;
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Distribution service unreachable when validating PartyId={Id}. Proceeding.", partyId);
                return true;
            }
        }
    }
}
