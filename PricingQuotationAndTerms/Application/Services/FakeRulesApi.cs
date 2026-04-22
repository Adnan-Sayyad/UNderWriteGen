using PricingQuotationAndTerms.Contracts.DTOs;
using PricingQuotationAndTerms.Contracts.Interfaces;

namespace PricingQuotationAndTerms.Application.Services;

/// <summary>
/// DEV STUB for the Rules/Scoring microservice (port 8085).
/// Returns varied risk scores based on the last byte of submissionId
/// so you can test different risk bands without needing the real service.
///
/// Replace with HttpRulesApi when teammate's module is ready.
/// </summary>
public class FakeRulesApi : IRulesApi
{
    public Task<RiskScoreDto?> GetRiskScoreAsync(Guid submissionId, CancellationToken ct = default)
    {
        // Use last byte of GUID to simulate different risk bands
        var lastByte = submissionId.ToByteArray()[15];

        (decimal score, string band) = lastByte switch
        {
            < 64  => (25m,  "Low"),           // 0–63   → Low
            < 128 => (62m,  "Medium"),         // 64–127 → Medium
            < 220 => (78m,  "High"),           // 128–219→ High
            _     => (95m,  "Unacceptable")    // 220–255→ Unacceptable
        };

        return Task.FromResult<RiskScoreDto?>(new RiskScoreDto
        {
            SubmissionId = submissionId,
            ScoreValue   = score,
            Band         = band,
            ModelVersion = "fake-v1.0"
        });
    }
}
