using System.Net.Http.Json;
using PricingQuotationAndTerms.Contracts.DTOs;
using PricingQuotationAndTerms.Contracts.Interfaces;

namespace PricingQuotationAndTerms.Infrastructure.ExternalApis;

/// <summary>
/// PRODUCTION implementation — calls the real Rules/Scoring microservice at port 8085.
/// Enable this in DependencyInjection.cs when teammate's module is ready.
/// </summary>
public class HttpRulesApi : IRulesApi
{
    private readonly HttpClient _http;
    private readonly ILogger<HttpRulesApi> _logger;

    public HttpRulesApi(HttpClient http, ILogger<HttpRulesApi> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task<RiskScoreDto?> GetRiskScoreAsync(Guid submissionId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync($"api/risk-scores/{submissionId}", ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Risk score not found for SubmissionId={Id}", submissionId);
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RiskScoreDto>(ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach Rules service for SubmissionId={Id}", submissionId);
            throw new InvalidOperationException(
                "Rules/Scoring service is unavailable. Cannot calculate risk score.", ex);
        }
    }
}
