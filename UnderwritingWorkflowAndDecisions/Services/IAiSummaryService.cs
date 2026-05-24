namespace UnderwritingWorkflowAndDecisions.Services
{
    public interface IAiSummaryService
    {
        Task<AiSummaryResult> SummarizeSubmissionAsync(Guid submissionId, CancellationToken ct = default);
    }

    public class AiSummaryResult
    {
        public string   Summary     { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }
}
