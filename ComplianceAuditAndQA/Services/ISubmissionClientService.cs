namespace ComplianceAuditAndQA.Services
{
    public interface ISubmissionClientService
    {
        Task<bool> SubmissionExistsAsync(Guid submissionId, CancellationToken ct = default);
    }
}
