using PricingQuotationAndTerms.Contracts.DTOs;
using PricingQuotationAndTerms.Contracts.Interfaces;

namespace PricingQuotationAndTerms.Application.Services;

/// <summary>
/// DEVELOPMENT ONLY stub for the Submission microservice.
/// Returns realistic fake data so our module can run independently.
///
/// In production → replaced by HttpSubmissionApi which calls the real service over HTTP.
/// Swap is done in DependencyInjection.cs — zero code change elsewhere.
/// </summary>
public class FakeSubmissionApi : ISubmissionApi
{
    public Task<SubmissionDto?> GetSubmissionByIdAsync(Guid submissionId, CancellationToken ct = default)
    {
        // Simulates what the real Submission microservice returns
        return Task.FromResult<SubmissionDto?>(new SubmissionDto
        {
            Id                 = submissionId,
            PartyId            = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            AgentId            = Guid.Parse("11111111-2222-3333-4444-555555555555"),
            ProductLine        = "Health",
            SumInsured         = 500_000m,
            PolicyTenureMonths = 12,
            OccupationType     = "IT",
            InceptionDate      = DateTime.UtcNow.AddDays(15),
            RiskScore          = 62,
            RiskBand           = "Medium",
            CoverageJson       = """{"hospitalization":true,"opd":false,"dental":false}""",
            IsRenewal          = false
        });
    }
}
