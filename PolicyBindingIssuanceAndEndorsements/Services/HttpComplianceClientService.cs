using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolicyBindingIssuanceAndEndorsements.Services
{
    public class HttpComplianceClientService : IComplianceClientService
    {
        private readonly HttpClient _http;
        private readonly ILogger<HttpComplianceClientService> _logger;
        private readonly string _internalKey;

        // DTO used only for serialising the default checklist items
        private sealed class CheckItem
        {
            [JsonPropertyName("item")]    public string Item    { get; init; } = string.Empty;
            [JsonPropertyName("checked")] public bool   Checked { get; init; }
        }

        // Standard 4-eyes compliance items auto-added to every bound policy checklist
        private static readonly string DefaultItemsJson = JsonSerializer.Serialize(new[]
        {
            new CheckItem { Item = "Verify policy number and submission ID match"     },
            new CheckItem { Item = "Confirm party identity documents are on file"     },
            new CheckItem { Item = "Verify sum insured is within authority limits"    },
            new CheckItem { Item = "Check all subjectivities have been met or waived" },
            new CheckItem { Item = "Confirm premium has been calculated and accepted"  },
            new CheckItem { Item = "Review inception and expiry dates are correct"    },
            new CheckItem { Item = "Ensure agent is licensed for this product line"   },
            new CheckItem { Item = "Final sign-off by Compliance officer"             },
        });

        public HttpComplianceClientService(
            HttpClient http,
            ILogger<HttpComplianceClientService> logger,
            IConfiguration config)
        {
            _http        = http;
            _logger      = logger;
            _internalKey = config["InternalServiceKey"] ?? string.Empty;
        }

        public async Task CreateChecklistAsync(Guid submissionId, CancellationToken ct = default)
        {
            try
            {
                var payload = new
                {
                    SubmissionId = submissionId,
                    ItemsJson    = DefaultItemsJson,
                    Status       = "Pending"
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, "api/compliance-checklists");
                request.Headers.Add("X-Internal-Service-Key", _internalKey);
                request.Content = JsonContent.Create(payload);

                var response = await _http.SendAsync(request, ct);
                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning("Compliance checklist creation returned {Code} for submission {Id}",
                        (int)response.StatusCode, submissionId);
                else
                    _logger.LogInformation("Compliance checklist auto-created for submission {Id}", submissionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create compliance checklist for submission {Id}. Non-blocking.", submissionId);
            }
        }
    }
}
