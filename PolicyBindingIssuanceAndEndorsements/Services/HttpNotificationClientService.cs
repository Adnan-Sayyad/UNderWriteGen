using System.Net.Http.Json;

namespace PolicyBindingIssuanceAndEndorsements.Services
{
    public class HttpNotificationClientService : INotificationClientService
    {
        private readonly HttpClient _http;
        private readonly ILogger<HttpNotificationClientService> _logger;

        public HttpNotificationClientService(HttpClient http, ILogger<HttpNotificationClientService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task SendAsync(string userId, string message, string category, CancellationToken ct = default)
        {
            try
            {
                var payload = new { UserID = userId, Message = message, Category = category };
                var response = await _http.PostAsJsonAsync("notifications", payload, ct);
                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning("Notification service returned {Status}", (int)response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification. Non-blocking.");
            }
        }
    }
}
