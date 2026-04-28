using PricingQuotationAndTerms.Contracts.DTOs;

namespace PricingQuotationAndTerms.Contracts.Interfaces;

/// <summary>
/// Public pricing contract. Lets other modules request a pricing calculation
/// without knowing HOW we price internally.
/// </summary>
public interface IPricingApi
{
    Task<PricingResultDto> CalculatePricingAsync(Guid submissionId, CancellationToken ct = default);
}
