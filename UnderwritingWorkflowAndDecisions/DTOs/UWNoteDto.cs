namespace UnderwritingWorkflowAndDecisions.DTOs
{
    public class CreateUWNoteDto
    {
        public Guid SubmissionID { get; set; }
        public Guid AuthorID { get; set; }
        public string NoteText { get; set; } = string.Empty;
    }

    public class UpdateUWNoteDto
    {
        public string NoteText { get; set; } = string.Empty;
    }
}
