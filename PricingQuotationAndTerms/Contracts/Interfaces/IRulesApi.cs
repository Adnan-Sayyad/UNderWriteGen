using PricingQuotationAndTerms.Contracts.DTOs;

namespace PricingQuotationAndTerms.Contracts.Interfaces;

/// <summary>
/// Contract for calling the Rules/Scoring microservice (port 8085).
/// Implemented by FakeRulesApi (dev) or HttpRulesApi (production).
/// </summary>
public interface IRulesApi
{
    Task<RiskScoreDto?> GetRiskScoreAsync(Guid submissionId, CancellationToken ct = default);
}
