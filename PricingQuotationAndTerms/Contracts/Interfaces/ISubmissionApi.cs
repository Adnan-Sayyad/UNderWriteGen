using PricingQuotationAndTerms.Contracts.DTOs;

namespace PricingQuotationAndTerms.Contracts.Interfaces;

/// <summary>
/// Contract for calling the Submission microservice.
/// Implemented by FakeSubmissionApi (dev) or HttpSubmissionApi (production).
/// </summary>
public interface ISubmissionApi
{
    Task<SubmissionDto?> GetSubmissionByIdAsync(Guid submissionId, CancellationToken ct = default);
}
