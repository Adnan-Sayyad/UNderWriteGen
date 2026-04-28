namespace UnderwritingWorkflowAndDecisions.DTOs
{
    public class CreateSubjectivityDto
    {
        public Guid SubmissionID { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
    }

    public class UpdateSubjectivityDto
    {
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
    }

    public class UpdateSubjectivityStatusDto
    {
        /// <summary>Open / Met / Waived</summary>
        public string Status { get; set; } = string.Empty;
    }
}
