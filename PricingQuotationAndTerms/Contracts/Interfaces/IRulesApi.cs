using PricingQuotationAndTerms.Contracts.DTOs;

namespace PricingQuotationAndTerms.Contracts.Interfaces;

/// <summary>
/// Contract for calling the Rules/Scoring microservice (port 8085).
/// Implemented by HttpRulesApi (production).
/// </summary>
public interface IRulesApi
{
    Task<RiskScoreDto?> GetRiskScoreAsync(Guid submissionId, CancellationToken ct = default);

    /// <summary>
    /// Triggers the Rules/Scoring service to calculate (or recalculate) the risk score
    /// for the given submission and persist it. Returns the resulting score.
    /// </summary>
    Task<RiskScoreDto> CalculateRiskScoreAsync(Guid submissionId, CancellationToken ct = default);
}
