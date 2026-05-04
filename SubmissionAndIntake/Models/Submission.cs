using SubmissionAndIntake.Configs.Enums;

namespace SubmissionAndIntake.Models
{
    public class Submission
    {
        public Guid SubmissionID { get; set; }
        public string PartyID { get; set; } = string.Empty;
        public string AgentID { get; set; } = string.Empty;
        public ProductLine ProductLine { get; set; }
        public string CoverageJSON { get; set; } = string.Empty;
        public DateTime InceptionDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public SubmissionStatus Status { get; set; }
    }
}
