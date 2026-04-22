using System.Text.Json;
using PricingQuotationAndTerms.Application.DTOs.External;
using PricingQuotationAndTerms.Application.Interfaces;
using PricingQuotationAndTerms.Contracts.DTOs;
using PricingQuotationAndTerms.Contracts.Interfaces;

namespace PricingQuotationAndTerms.Application.Services;

/// <summary>
/// Bridges the public IPricingApi contract (used by other modules)
/// to our internal IPricingService (which has richer input types).
///
/// Why this exists:
///   IPricingApi only takes a submissionId (simple contract for callers).
///   IPricingService needs full SubmissionPricingInput + RiskScoreInput.
///   This adapter fetches the submission and builds the full input objects.
/// </summary>
public class PricingApiAdapter : IPricingApi
{
    private readonly IPricingService _pricingService;
    private readonly ISubmissionApi  _submissionApi;

    public PricingApiAdapter(IPricingService pricingService, ISubmissionApi submissionApi)
    {
        _pricingService = pricingService;
        _submissionApi  = submissionApi;
    }

    public async Task<PricingResultDto> CalculatePricingAsync(Guid submissionId, CancellationToken ct = default)
    {
        var submission = await _submissionApi.GetSubmissionByIdAsync(submissionId, ct)
            ?? throw new KeyNotFoundException($"Submission '{submissionId}' not found.");

        var pricingInput = new SubmissionPricingInput
        {
            SubmissionId       = submission.Id,
            ProductLine        = submission.ProductLine,
            SumInsured         = submission.SumInsured,
            PolicyTenureMonths = submission.PolicyTenureMonths,
            OccupationType     = submission.OccupationType,
            IsRenewal          = submission.IsRenewal
        };

        var riskInput = new RiskScoreInput
        {
            SubmissionId = submission.Id,
            ScoreValue   = submission.RiskScore,
            Band         = submission.RiskBand
        };

        var result = await _pricingService.CalculatePremiumAsync(pricingInput, riskInput, ct);

        return new PricingResultDto
        {
            BasePremium  = result.BasePremium,
            TotalPremium = result.TotalPremium,
            LoadingsJson = JsonSerializer.Serialize(new
            {
                result.RiskLoading,
                result.OccupationLoading
            }),
            DiscountsJson = JsonSerializer.Serialize(new
            {
                result.TenureDiscount,
                result.LoyaltyDiscount,
                result.AgentDiscount
            }),
            TaxesJson    = JsonSerializer.Serialize(new { GstAmount = result.TaxAmount }),
            PricingNotes = result.PricingNotes
        };
    }
}
