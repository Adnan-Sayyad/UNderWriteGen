namespace UnderwritingWorkflowAndDecisions.Services
{
    public interface ISubmissionClientService
    {
        Task UpdateStatusAsync(Guid submissionId, string status, CancellationToken ct = default);
        Task<SubmissionSummaryData?> GetSubmissionAsync(Guid submissionId, CancellationToken ct = default);
    }

    public class SubmissionSummaryData
    {
        public string  ProductLine        { get; set; } = string.Empty;
        public decimal SumInsured         { get; set; }
        public string  OccupationType     { get; set; } = string.Empty;
        public int     PolicyTenureMonths { get; set; }
        public string  Status             { get; set; } = string.Empty;
        public DateTime InceptionDate     { get; set; }
    }
}
