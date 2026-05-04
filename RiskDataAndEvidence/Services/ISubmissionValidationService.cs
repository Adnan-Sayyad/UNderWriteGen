namespace RiskDataAndEvidence.Services;

public interface ISubmissionValidationService
{
    Task<bool> SubmissionExistsAsync(Guid submissionId, CancellationToken ct = default);
}
