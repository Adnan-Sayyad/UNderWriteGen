using SubmissionAndIntake.Configs.Enums;

namespace SubmissionAndIntake.Models
{
    public class Attachment
    {
        public Guid AttachmentID { get; set; }
        public Guid SubmissionID { get; set; }
        public DocType DocType { get; set; }
        public string FileURI { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = string.Empty;
        public DateTime UploadedDate { get; set; }
    }
}
