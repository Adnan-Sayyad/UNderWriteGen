using System.Net.Http.Json;
using PricingQuotationAndTerms.Application.Interfaces;

namespace PricingQuotationAndTerms.Infrastructure.ExternalApis
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

        public async Task SendAsync(string recipientEmail, string message, string category, CancellationToken ct = default)
        {
            try
            {
                var payload = new { RecipientEmail = recipientEmail, Message = message, Category = category };

                using var request = new HttpRequestMessage(HttpMethod.Post, "api/notifications");
                request.Headers.Add("X-Service-Key", _serviceKey);
                request.Content = JsonContent.Create(payload);

                var response = await _http.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning("Notification service returned {Status} for {Email}", (int)response.StatusCode, recipientEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to {Email}. Non-blocking.", recipientEmail);
            }
        }

        public async Task BroadcastAsync(string recipientGroup, string message, string category, CancellationToken ct = default)
        {
            try
            {
                var payload = new { RecipientGroup = recipientGroup, Message = message, Category = category };

                using var request = new HttpRequestMessage(HttpMethod.Post, "api/notifications/broadcast");
                request.Headers.Add("X-Service-Key", _serviceKey);
                request.Content = JsonContent.Create(payload);

                var response = await _http.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning("Notification broadcast returned {Status} for group {Group}", (int)response.StatusCode, recipientGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast notification to group {Group}. Non-blocking.", recipientGroup);
            }
        }
    }
}
