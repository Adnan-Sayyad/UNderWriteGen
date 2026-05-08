using System.Net.Http.Json;

namespace UnderwritingWorkflowAndDecisions.Services
{
    public class HttpNotificationClientService : INotificationClientService
    {
        private readonly HttpClient _http;
        private readonly ILogger<HttpNotificationClientService> _logger;
        private readonly string _serviceKey;

        public HttpNotificationClientService(
            HttpClient http,
            ILogger<HttpNotificationClientService> logger,
            IConfiguration config)
        {
            _http = http;
            _logger = logger;
            _serviceKey = config["InternalServiceKey"] ?? string.Empty;
        }

        public async Task SendAsync(string mail, string message, string category, CancellationToken ct = default)
        {
            try
            {
                var payload = new { Mail = mail, Message = message, Category = category };

                using var request = new HttpRequestMessage(HttpMethod.Post, "notifications");
                request.Headers.Add("X-Service-Key", _serviceKey);
                request.Content = JsonContent.Create(payload);

                var response = await _http.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning("Notification service returned {Status} for {Mail}", (int)response.StatusCode, mail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to {Mail}. Non-blocking.", mail);
            }
        }
    }
}
