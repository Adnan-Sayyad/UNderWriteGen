using SubmissionAndIntake.Configs.Enums;

namespace SubmissionAndIntake.Models
{
    public class CompletenessCheck
    {
        public Guid CheckID { get; set; }
        public Guid SubmissionID { get; set; }
        public string MissingItemsJSON { get; set; } = string.Empty;
        public CheckStatus Status { get; set; }
        public DateTime? CheckedDate { get; set; }
    }
}
