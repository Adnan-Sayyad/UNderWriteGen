using System.Net.Http.Json;
using PricingQuotationAndTerms.Contracts.DTOs;
using PricingQuotationAndTerms.Contracts.Interfaces;

namespace PricingQuotationAndTerms.Infrastructure.ExternalApis;

/// <summary>
/// PRODUCTION implementation — calls the real Submission microservice at port 8083.
/// Enable this in DependencyInjection.cs when teammate's module is ready.
/// </summary>
public class HttpSubmissionApi : ISubmissionApi
{
    private readonly HttpClient _http;
    private readonly ILogger<HttpSubmissionApi> _logger;

    public HttpSubmissionApi(HttpClient http, ILogger<HttpSubmissionApi> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task<SubmissionDto?> GetSubmissionByIdAsync(Guid submissionId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync($"api/submissions/{submissionId}", ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Submission not found: {Id}", submissionId);
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SubmissionDto>(ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach Submission service for Id={Id}", submissionId);
            throw new InvalidOperationException(
                "Submission service is unavailable. Cannot retrieve submission.", ex);
        }
    }
}
