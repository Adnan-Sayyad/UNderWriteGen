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
/// Flow:
///   1. Fetch submission data from SubmissionAndIntake service.
///   2. Fetch the risk score from Rules/Scoring service.
///      If no score exists yet, trigger calculation first.
///   3. Pass both to PricingService to produce the premium breakdown.
/// </summary>
public class PricingApiAdapter : IPricingApi
{
    private readonly IPricingService _pricingService;
    private readonly ISubmissionApi  _submissionApi;
    private readonly IRulesApi       _rulesApi;

    public PricingApiAdapter(
        IPricingService pricingService,
        ISubmissionApi  submissionApi,
        IRulesApi       rulesApi)
    {
        _pricingService = pricingService;
        _submissionApi  = submissionApi;
        _rulesApi       = rulesApi;
    }

    public async Task<PricingResultDto> CalculatePricingAsync(Guid submissionId, CancellationToken ct = default)
    {
        var submission = await _submissionApi.GetSubmissionByIdAsync(submissionId, ct)
            ?? throw new KeyNotFoundException($"Submission '{submissionId}' not found.");

        // Get the risk score from the Rules/Scoring service.
        // If no score has been calculated yet, trigger the calculation now.
        var riskScoreDto = await _rulesApi.GetRiskScoreAsync(submissionId, ct);
        if (riskScoreDto is null)
            riskScoreDto = await _rulesApi.CalculateRiskScoreAsync(submissionId, ct);

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
            SubmissionId = submissionId,
            ScoreValue   = riskScoreDto.ScoreValue,
            Band         = riskScoreDto.Band,
            ModelVersion = riskScoreDto.ModelVersion
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
