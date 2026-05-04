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

    public async Task<AgentDto?> GetAgentByIdAsync(string agentId, CancellationToken ct = default)
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

            // Map Distribution service AgentResponseDto → AgentDto
            var raw = await response.Content.ReadFromJsonAsync<AgentRaw>(
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

            if (raw?.Data is null) return null;

            return new AgentDto
            {
                AgentId          = raw.Data.AgentID,
                ProducerCode     = raw.Data.ProducerCode,
                Region           = raw.Data.Region ?? string.Empty,
                IsPreferredAgent = raw.Data.Status == "Active",
                CommissionRate   = 0.05m
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach Distribution service for AgentId={Id}", agentId);
            return null;
        }
    }

    // Matches the ApiResponse<AgentResponseDto> wrapper from Distribution service
    private sealed class AgentRaw
    {
        public AgentRawData? Data { get; set; }
    }
    private sealed class AgentRawData
    {
        public string  AgentID      { get; set; } = string.Empty;
        public string  ProducerCode { get; set; } = string.Empty;
        public string? Region       { get; set; }
        public string  Status       { get; set; } = string.Empty;
    }
}
