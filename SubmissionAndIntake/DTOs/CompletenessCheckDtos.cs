using SubmissionAndIntake.Configs.Enums;

namespace SubmissionAndIntake.DTOs
{
    public class CreateCompletenessCheckDto
    {
        public Guid SubmissionID { get; set; }
        public string MissingItemsJSON { get; set; } = string.Empty;
    }

    public class UpdateCompletenessCheckDto
    {
        public string MissingItemsJSON { get; set; } = string.Empty;
        public CheckStatus Status { get; set; }
        public DateTime? CheckedDate { get; set; }
    }

    public class CompletenessCheckResponseDto
    {
        public Guid CheckID { get; set; }
        public Guid SubmissionID { get; set; }
        public string MissingItemsJSON { get; set; } = string.Empty;
        public CheckStatus Status { get; set; }
        public DateTime? CheckedDate { get; set; }
    }
}
