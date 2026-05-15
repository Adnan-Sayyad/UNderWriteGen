using System.Net;
using System.Net.Http.Headers;

namespace SubmissionAndIntake.Services
{
    public class HttpDistributionValidationService : IDistributionValidationService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<HttpDistributionValidationService> _logger;

        public HttpDistributionValidationService(
            HttpClient http,
            IHttpContextAccessor httpContextAccessor,
            ILogger<HttpDistributionValidationService> logger)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> AgentExistsAsync(string agentId, CancellationToken ct = default)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"agents/{agentId}");
                ForwardToken(request);
                var response = await _http.SendAsync(request, ct);
                if (response.StatusCode == HttpStatusCode.NotFound) return false;
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Distribution service returned 401 for AgentId={Id}. Proceeding.", agentId);
                    return true;
                }
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
                var request = new HttpRequestMessage(HttpMethod.Get, $"customerparties/{partyId}");
                ForwardToken(request);
                var response = await _http.SendAsync(request, ct);
                if (response.StatusCode == HttpStatusCode.NotFound) return false;
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Distribution service returned 401 for PartyId={Id}. Proceeding.", partyId);
                    return true;
                }
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Distribution service unreachable when validating PartyId={Id}. Proceeding.", partyId);
                return true;
            }
        }

        private void ForwardToken(HttpRequestMessage request)
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = AuthenticationHeaderValue.Parse(token);
        }
    }
}
