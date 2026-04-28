using System.Net;
using System.Net.Http.Json;
using PricingQuotationAndTerms.Contracts.DTOs;
using PricingQuotationAndTerms.Contracts.Interfaces;

namespace PricingQuotationAndTerms.Infrastructure.ExternalApis;

public class HttpAgentApi : IAgentApi
{
    private readonly HttpClient _http;
    private readonly ILogger<HttpAgentApi> _logger;

    public HttpAgentApi(HttpClient http, ILogger<HttpAgentApi> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<AgentDto?> GetAgentByIdAsync(Guid agentId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync($"agents/{agentId}", ct);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Agent not found: {Id}", agentId);
                return null;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AgentDto>(ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach Distribution service for AgentId={Id}", agentId);
            return null;
        }
    }
}
