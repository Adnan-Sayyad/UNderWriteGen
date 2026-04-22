namespace UnderwritingWorkflowAndDecisions.DTOs
{
    public class CreateUWDecisionDto
    {
        public Guid SubmissionID { get; set; }

        /// <summary>Approve / Decline / Refer / MoreInfo</summary>
        public string Decision { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;
        public Guid DecidedBy { get; set; }
    }

    public class UpdateUWDecisionDto
    {
        public string Decision { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
