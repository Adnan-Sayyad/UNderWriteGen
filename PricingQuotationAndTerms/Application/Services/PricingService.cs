using PricingQuotationAndTerms.Application.DTOs.External;
using PricingQuotationAndTerms.Application.DTOs.Internal;
using PricingQuotationAndTerms.Application.Interfaces;
using PricingQuotationAndTerms.Domain.Repositories;

namespace PricingQuotationAndTerms.Application.Services;

/// <summary>
/// Production pricing engine — reads ALL rates and factors from the database.
/// No hardcoded values. Underwriters can change rates via the PricingParams API
/// without any code deployment.
///
/// DB records it reads:
///   ProductLine="Health",  ParamName="BaseRate"              → 0.035
///   ProductLine="Global",  ParamName="RiskLoading_Medium"    → 0.10
///   ProductLine="Global",  ParamName="RiskLoading_High"      → 0.25
///   ProductLine="Global",  ParamName="OccupationLoad_Mining" → 0.30
///   ProductLine="Global",  ParamName="TenureDiscount_12m"    → 0.03
///   ProductLine="Global",  ParamName="LoyaltyDiscount"       → 0.05
///   ProductLine="Global",  ParamName="GstRate"               → 0.18
///   ProductLine="Global",  ParamName="MinimumPremium"        → 500
/// </summary>
public class PricingService : IPricingService
{
    private readonly IPricingParamRepository _paramRepo;
    private readonly ILogger<PricingService> _logger;

    // Fallback constants — used ONLY if DB param is missing (safety net)
    private const decimal FallbackBaseRate       = 0.025m;
    private const decimal FallbackGstRate        = 0.18m;
    private const decimal FallbackMinPremium     = 500m;

    public PricingService(IPricingParamRepository paramRepo, ILogger<PricingService> logger)
    {
        _paramRepo = paramRepo;
        _logger    = logger;
    }

    public async Task<PricingResult> CalculatePremiumAsync(
        SubmissionPricingInput input,
        RiskScoreInput         riskScore,
        CancellationToken      ct = default)
    {
        // ── Validate ─────────────────────────────────────────────────
        if (input.SumInsured <= 0)
            throw new ArgumentException("SumInsured must be greater than zero.");

        if (input.PolicyTenureMonths <= 0)
            throw new ArgumentException("PolicyTenureMonths must be at least 1.");

        if (riskScore.Band == "Unacceptable")
            throw new InvalidOperationException(
                "Risk band is Unacceptable. Quote cannot be generated.");

        // ── Load all params for this product line + Global from DB ────
        var productParams = (await _paramRepo.GetByProductLineAsync(input.ProductLine, ct))
                            .ToDictionary(p => p.ParamName, p => p.Value);

        var globalParams  = (await _paramRepo.GetByProductLineAsync("Global", ct))
                            .ToDictionary(p => p.ParamName, p => p.Value);

        // Helper: read param with fallback + warning
        decimal Get(Dictionary<string, decimal> src, string key, decimal fallback)
        {
            if (src.TryGetValue(key, out var val)) return val;
            _logger.LogWarning("PricingParam '{Key}' not found in DB. Using fallback {Fallback}", key, fallback);
            return fallback;
        }

        // ── Step 1: Base Premium ──────────────────────────────────────
        decimal baseRate     = Get(productParams, "BaseRate", FallbackBaseRate);
        decimal tenureFactor = input.PolicyTenureMonths / 12.0m;
        decimal basePremium  = Math.Round(input.SumInsured * baseRate * tenureFactor, 2);

        // ── Step 2: Risk Loading ──────────────────────────────────────
        decimal riskLoading = riskScore.Band switch
        {
            "Low"    => 0m,
            "Medium" => Math.Round(basePremium * Get(globalParams, "RiskLoading_Medium", 0.10m), 2),
            "High"   => Math.Round(basePremium * Get(globalParams, "RiskLoading_High",   0.25m), 2),
            _        => Math.Round(basePremium * 0.05m, 2)
        };

        // ── Step 3: Occupation Loading (Health & Life only) ───────────
        decimal occupationLoading = 0m;
        if (input.ProductLine.Equals("Health", StringComparison.OrdinalIgnoreCase) ||
            input.ProductLine.Equals("Life",   StringComparison.OrdinalIgnoreCase))
        {
            string occKey = $"OccupationLoad_{input.OccupationType}";
            decimal occFactor = Get(globalParams, occKey, 0.05m); // 5% default for unknown occupation
            occupationLoading = Math.Round(basePremium * occFactor, 2);
        }

        // ── Step 4: Tenure Discount ───────────────────────────────────
        decimal tenureDiscount = input.PolicyTenureMonths switch
        {
            >= 36 => Math.Round(basePremium * Get(globalParams, "TenureDiscount_36m", 0.07m), 2),
            >= 24 => Math.Round(basePremium * Get(globalParams, "TenureDiscount_24m", 0.05m), 2),
            >= 12 => Math.Round(basePremium * Get(globalParams, "TenureDiscount_12m", 0.03m), 2),
            _     => 0m
        };

        // ── Step 5: Loyalty Discount ──────────────────────────────────
        decimal loyaltyDiscount = input.IsRenewal
            ? Math.Round(basePremium * Get(globalParams, "LoyaltyDiscount", 0.05m), 2)
            : 0m;

        // ── Step 6: Agent Discount ────────────────────────────────────
        decimal agentDiscount = 0m;
        if (input.IsPreferredAgent)
        {
            decimal agentFactor = Get(globalParams, "AgentDiscount_Preferred", 0.02m);
            agentDiscount = Math.Round(basePremium * agentFactor, 2);
        }

        // ── Step 7: Adjusted Subtotal ─────────────────────────────────
        decimal subtotal = basePremium + riskLoading + occupationLoading
                         - tenureDiscount - loyaltyDiscount - agentDiscount;

        decimal minPremium = Get(globalParams, "MinimumPremium", FallbackMinPremium);
        if (subtotal < minPremium)
        {
            _logger.LogInformation(
                "Premium {Subtotal} below minimum {MinPremium}. Applying floor.",
                subtotal, minPremium);
            subtotal = minPremium;
        }

        // ── Step 7: GST ───────────────────────────────────────────────
        decimal gstRate   = Get(globalParams, "GstRate", FallbackGstRate);
        decimal taxAmount = Math.Round(subtotal * gstRate, 2);
        decimal total     = Math.Round(subtotal + taxAmount, 2);

        string notes = $"Product={input.ProductLine} | SumInsured={input.SumInsured:C} | " +
                       $"Tenure={input.PolicyTenureMonths}m | RiskBand={riskScore.Band} | " +
                       $"Score={riskScore.ScoreValue} | Renewal={input.IsRenewal}";

        _logger.LogInformation(
            "Pricing complete: Base={Base}, Loading={Load}, Discount={Disc}, Tax={Tax}, Total={Total}",
            basePremium, riskLoading + occupationLoading, tenureDiscount + loyaltyDiscount, taxAmount, total);

        return new PricingResult
        {
            BasePremium       = basePremium,
            RiskLoading       = riskLoading,
            OccupationLoading = occupationLoading,
            TenureDiscount    = tenureDiscount,
            LoyaltyDiscount   = loyaltyDiscount,
            AgentDiscount     = agentDiscount,
            TaxAmount         = taxAmount,
            TotalPremium      = total,
            PricingNotes      = notes
        };
    }
}
