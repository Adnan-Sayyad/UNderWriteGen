using PricingQuotationAndTerms.Application.DTOs.External;
using PricingQuotationAndTerms.Application.DTOs.Internal;

namespace PricingQuotationAndTerms.Application.Interfaces;

/// <summary>
/// INTERNAL interface — only used inside this module.
/// Has full input objects (unlike the public IPricingApi which only takes a submissionId).
/// </summary>
public interface IPricingService
{
    Task<PricingResult> CalculatePremiumAsync(
        SubmissionPricingInput input,
        RiskScoreInput         riskScore,
        CancellationToken      ct = default);
}
