namespace UnderwritingWorkflowAndDecisions.Models
{
    public class UWDecision
    {
        public Guid DecisionID { get; set; } = Guid.NewGuid();
        public Guid SubmissionID { get; set; }

        /// <summary>Approve / Decline / Refer / MoreInfo</summary>
        public string Decision { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;
        public Guid DecidedBy { get; set; }
        public DateTime DecidedDate { get; set; } = DateTime.UtcNow;
    }
}
