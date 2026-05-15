namespace UnderwritingWorkflowAndDecisions.Services
{
    public interface ISubmissionClientService
    {
        Task UpdateStatusAsync(Guid submissionId, string status, CancellationToken ct = default);
    }
}
