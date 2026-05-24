using RulesScoringAndReferralMatrix.DTOs;

namespace RulesScoringAndReferralMatrix.Services
{
    public interface ISubmissionClientService
    {
        Task<SubmissionDataDto?> GetSubmissionAsync(Guid submissionId, CancellationToken ct = default);
    }
}
