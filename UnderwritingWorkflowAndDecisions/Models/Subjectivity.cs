namespace UnderwritingWorkflowAndDecisions.Models
{
    public class Subjectivity
    {
        public Guid SubjectivityID { get; set; } = Guid.NewGuid();
        public Guid SubmissionID { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }

        /// <summary>Open / Met / Waived</summary>
        public string Status { get; set; } = "Open";
    }
}
