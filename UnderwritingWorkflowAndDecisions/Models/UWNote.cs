namespace UnderwritingWorkflowAndDecisions.Models
{
    public class UWNote
    {
        public Guid NoteID { get; set; } = Guid.NewGuid();
        public Guid SubmissionID { get; set; }
        public Guid AuthorID { get; set; }
        public string NoteText { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
